using Agriwise.Models;
using Agriwise.Services;

namespace Agriwise.Views;

public partial class DashboardPage : ContentPage
{
    private List<Parcel> _allParcels = new();
    private string _currentFilter = "Toutes les parcelles";

    public DashboardPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await RefreshDashboardAsync();
    }

    private async Task RefreshDashboardAsync()
    {
        await DataService.Current.InitializeAsync();
        _allParcels = await DataService.Current.GetParcelsAsync();

        // 1. Update Metrics
        TotalPlotsCountLabel.Text = _allParcels.Count.ToString();
        TotalAreaLabel.Text = $"{_allParcels.Sum(p => p.AreaHa):0.0} ha total";
        
        var readyCount = _allParcels.Count(p => !p.HasActiveDar || p.DarDaysRemaining <= 0);
        ReadyPlotsCountLabel.Text = readyCount.ToString();

        var darPlots = _allParcels.Where(p => p.HasActiveDar && p.DarDaysRemaining > 0).ToList();
        DarPlotsCountLabel.Text = darPlots.Count.ToString();

        // 2. Update DAR Safety Hero Banner
        if (darPlots.Count > 0)
        {
            var p = darPlots.First();
            DarBannerBorder.IsVisible = true;
            DarBannerBorder.Stroke = Color.FromArgb("#FAD7B2");
            DarRemainingLabel.Text = $"J-{p.DarDaysRemaining} restant";
            DarTitleLabel.Text = $"Délai Avant Récolte (DAR) actif sur {p.Name}";
            
            var chemical = string.IsNullOrWhiteSpace(p.LastChemicalApplied) ? "Fongicide homologué" : p.LastChemicalApplied;
            DarDescriptionLabel.Text = $"Traitement {chemical} appliqué. Récolte interdite pendant encore {p.DarDaysRemaining} jours pour éliminer tout résidu toxique.";
            
            var dayCurrent = Math.Max(1, p.DarDaysTotal - p.DarDaysRemaining);
            DarDaysMeterLabel.Text = $"Jour {dayCurrent} / {p.DarDaysTotal}";
            DarProgressBarBox.WidthRequest = Math.Clamp((p.DarProgressPercent / 100.0) * 160, 20, 160);
            
            if (p.DarHarvestAuthorizedDate.HasValue)
            {
                DarDateEndLabel.Text = $"Récolte autorisée le {p.DarHarvestAuthorizedDate.Value:dd MMM}";
            }
        }
        else
        {
            DarRemainingLabel.Text = "Sécurisé";
            DarRemainingLabel.TextColor = Color.FromArgb("#27AE60");
            DarTitleLabel.Text = "Toutes les parcelles sont sécurisées pour la récolte";
            DarDescriptionLabel.Text = "Aucun délai de rémanence chimique en cours. Les normes de sécurité sanitaire sont respectées sur toute l'exploitation.";
            DarDaysMeterLabel.Text = "100% Conforme";
            DarProgressBarBox.Color = Color.FromArgb("#27AE60");
            DarProgressBarBox.WidthRequest = 160;
            DarDateEndLabel.Text = "Récolte autorisée dès aujourd'hui";
        }

        // 3. Render Parcels Feed
        ApplyFilterAndRender();
    }

    private void ApplyFilterAndRender()
    {
        IEnumerable<Parcel> filtered = _allParcels;

        switch (_currentFilter)
        {
            case "Tomates":
                filtered = filtered.Where(p => p.Crop.Contains("Tomate", StringComparison.OrdinalIgnoreCase));
                break;
            case "Cacao":
                filtered = filtered.Where(p => p.Crop.Contains("Cacao", StringComparison.OrdinalIgnoreCase));
                break;
            case "Maïs":
                filtered = filtered.Where(p => p.Crop.Contains("Maïs", StringComparison.OrdinalIgnoreCase) || p.Crop.Contains("Mais", StringComparison.OrdinalIgnoreCase));
                break;
            case "En délai DAR uniquement":
                filtered = filtered.Where(p => p.HasActiveDar && p.DarDaysRemaining > 0);
                break;
            case "Prêtes à récolter":
                filtered = filtered.Where(p => !p.HasActiveDar || p.DarDaysRemaining <= 0);
                break;
        }

        RenderParcels(filtered.ToList());
    }

    private void RenderParcels(List<Parcel> parcels)
    {
        ParcelsContainer.Children.Clear();

        if (parcels.Count == 0)
        {
            var emptyLabel = new Label
            {
                Text = "Aucune parcelle ne correspond à ce filtre.",
                TextColor = Color.FromArgb("#71796F"),
                FontSize = 13,
                HorizontalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 20)
            };
            ParcelsContainer.Children.Add(emptyLabel);
            return;
        }

        foreach (var parcel in parcels)
        {
            var card = CreateParcelCard(parcel);
            ParcelsContainer.Children.Add(card);
        }
    }

    private Border CreateParcelCard(Parcel parcel)
    {
        var isDarActive = parcel.HasActiveDar && parcel.DarDaysRemaining > 0;
        var borderStrokeColor = isDarActive ? Color.FromArgb("#FAD7B2") : Color.FromArgb("#E5E7EB");
        var strokeThickness = isDarActive ? 1.5 : 1.0;
        var accentColor = Color.FromArgb(parcel.ColorAccentHex ?? (isDarActive ? "#E67E22" : "#27AE60"));

        var cardBorder = new Border
        {
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(16) },
            BackgroundColor = Colors.White,
            Stroke = borderStrokeColor,
            StrokeThickness = strokeThickness,
            Padding = new Thickness(16, 14)
        };

        var mainGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = 5 },
                new ColumnDefinition { Width = GridLength.Star }
            },
            ColumnSpacing = 12
        };

        // Left crop indicator bar
        var bar = new BoxView
        {
            Color = accentColor,
            CornerRadius = 3,
            VerticalOptions = LayoutOptions.Fill
        };
        Grid.SetColumn(bar, 0);
        mainGrid.Children.Add(bar);

        // Content Column
        var contentStack = new VerticalStackLayout { Spacing = 10 };
        Grid.SetColumn(contentStack, 1);

        // Header Row: Title + Area Pill + Status Pill
        var headerGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto }
            }
        };

        var titleStack = new VerticalStackLayout { Spacing = 2 };
        var nameAndAreaStack = new HorizontalStackLayout { Spacing = 6 };
        nameAndAreaStack.Children.Add(new Label
        {
            Text = parcel.Name,
            FontSize = 15,
            FontFamily = "OpenSansSemibold",
            TextColor = Color.FromArgb("#013A13")
        });

        var areaBorder = new Border
        {
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(8) },
            BackgroundColor = Color.FromArgb("#F8F9FA"),
            Stroke = Color.FromArgb("#E5E7EB"),
            StrokeThickness = 1,
            Padding = new Thickness(6, 2)
        };
        areaBorder.Content = new Label
        {
            Text = $"{parcel.AreaHa:0.0} ha",
            FontSize = 11,
            TextColor = Color.FromArgb("#414940")
        };
        nameAndAreaStack.Children.Add(areaBorder);
        titleStack.Children.Add(nameAndAreaStack);

        titleStack.Children.Add(new Label
        {
            Text = $"{parcel.Location} · Variété {parcel.Variety}",
            FontSize = 11,
            TextColor = Color.FromArgb("#71796F")
        });
        Grid.SetColumn(titleStack, 0);
        headerGrid.Children.Add(titleStack);

        // Status Badge
        var statusBorder = new Border
        {
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(12) },
            BackgroundColor = isDarActive ? Color.FromArgb("#FEF5EC") : Color.FromArgb("#EAF7EE"),
            StrokeThickness = 0,
            Padding = new Thickness(8, 4)
        };
        var statusStack = new HorizontalStackLayout { Spacing = 4, VerticalOptions = LayoutOptions.Center };
        statusStack.Children.Add(new Image
        {
            Source = isDarActive ? "ic_timer.png" : "ic_check.png",
            WidthRequest = 12,
            HeightRequest = 12,
            VerticalOptions = LayoutOptions.Center
        });
        statusStack.Children.Add(new Label
        {
            Text = isDarActive ? $"En attente · {parcel.DarDaysRemaining}j" : "Récolte autorisée",
            FontSize = 11,
            TextColor = isDarActive ? Color.FromArgb("#E67E22") : Color.FromArgb("#27AE60"),
            FontFamily = "OpenSansSemibold",
            VerticalOptions = LayoutOptions.Center
        });
        statusBorder.Content = statusStack;
        Grid.SetColumn(statusBorder, 1);
        headerGrid.Children.Add(statusBorder);

        contentStack.Children.Add(headerGrid);

        // Treatment details box
        var detailsBorder = new Border
        {
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(10) },
            BackgroundColor = isDarActive ? Color.FromArgb("#FEF5EC") : Color.FromArgb("#F8F9FA"),
            Stroke = isDarActive ? Color.FromArgb("#FAD7B2") : Color.FromArgb("#E5E7EB"),
            StrokeThickness = 1,
            Padding = new Thickness(10, 8)
        };

        var detailStack = new VerticalStackLayout { Spacing = 4 };
        if (isDarActive)
        {
            var row1 = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto }
                }
            };
            row1.Children.Add(new Label
            {
                Text = string.IsNullOrWhiteSpace(parcel.LastChemicalApplied) ? "Traitement DAR actif" : parcel.LastChemicalApplied,
                FontSize = 12,
                FontFamily = "OpenSansSemibold",
                TextColor = Color.FromArgb("#E67E22")
            });
            
            var dosePill = new Border
            {
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(6) },
                BackgroundColor = Colors.White,
                Stroke = Color.FromArgb("#FAD7B2"),
                StrokeThickness = 1,
                Padding = new Thickness(6, 2)
            };
            dosePill.Content = new Label
            {
                Text = "Dose respectée",
                FontSize = 10,
                TextColor = Color.FromArgb("#E67E22"),
                FontFamily = "OpenSansSemibold"
            };
            Grid.SetColumn(dosePill, 1);
            row1.Children.Add(dosePill);
            detailStack.Children.Add(row1);

            detailStack.Children.Add(new Label
            {
                Text = $"Délai Avant Récolte obligatoire : {parcel.DarDaysTotal} jours.",
                FontSize = 11,
                TextColor = Color.FromArgb("#414940")
            });

            var progressGrid = new Grid { HeightRequest = 6 };
            progressGrid.Children.Add(new BoxView { CornerRadius = 3, Color = Color.FromArgb("#E5E7EB") });
            progressGrid.Children.Add(new BoxView
            {
                CornerRadius = 3,
                Color = Color.FromArgb("#E67E22"),
                HorizontalOptions = LayoutOptions.Start,
                WidthRequest = Math.Clamp((parcel.DarProgressPercent / 100.0) * 180, 15, 180)
            });
            detailStack.Children.Add(progressGrid);
        }
        else
        {
            var row1 = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto }
                }
            };
            row1.Children.Add(new Label
            {
                Text = $"Dernier traitement : {(string.IsNullOrWhiteSpace(parcel.LastChemicalApplied) ? "Entretien régulier" : parcel.LastChemicalApplied)}",
                FontSize = 11,
                TextColor = Color.FromArgb("#414940")
            });
            if (parcel.LastTreatmentDate.HasValue)
            {
                var dateLbl = new Label
                {
                    Text = $"{parcel.LastTreatmentDate.Value:dd MMM}",
                    FontSize = 11,
                    TextColor = Color.FromArgb("#71796F")
                };
                Grid.SetColumn(dateLbl, 1);
                row1.Children.Add(dateLbl);
            }
            detailStack.Children.Add(row1);

            detailStack.Children.Add(new Label
            {
                Text = "Sécurité résidus : Conforme aux normes zéro délai phytosanitaire",
                FontSize = 11,
                TextColor = Color.FromArgb("#1A6D0E"),
                FontFamily = "OpenSansSemibold"
            });
        }
        detailsBorder.Content = detailStack;
        contentStack.Children.Add(detailsBorder);

        // Actions Row
        var actionGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto }
            }
        };

        var leftNotice = new HorizontalStackLayout { Spacing = 4, VerticalOptions = LayoutOptions.Center };
        if (isDarActive)
        {
            leftNotice.Children.Add(new Image { Source = "ic_alert.png", WidthRequest = 12, HeightRequest = 12, VerticalOptions = LayoutOptions.Center });
            leftNotice.Children.Add(new Label
            {
                Text = "Vente marché bloquée",
                FontSize = 11,
                TextColor = Color.FromArgb("#BA1A1A"),
                FontFamily = "OpenSansSemibold",
                VerticalOptions = LayoutOptions.Center
            });
        }
        else
        {
            leftNotice.Children.Add(new Image { Source = "ic_check.png", WidthRequest = 12, HeightRequest = 12, VerticalOptions = LayoutOptions.Center });
            leftNotice.Children.Add(new Label
            {
                Text = "Prête pour le marché",
                FontSize = 11,
                TextColor = Color.FromArgb("#27AE60"),
                FontFamily = "OpenSansSemibold",
                VerticalOptions = LayoutOptions.Center
            });
        }
        Grid.SetColumn(leftNotice, 0);
        actionGrid.Children.Add(leftNotice);

        var actionBtn = new Button
        {
            Text = isDarActive ? "Fiche intrant" : "Journal de récolte",
            BackgroundColor = isDarActive ? Color.FromArgb("#FEF5EC") : Color.FromArgb("#F8F9FA"),
            TextColor = isDarActive ? Color.FromArgb("#E67E22") : Color.FromArgb("#013A13"),
            BorderColor = isDarActive ? Color.FromArgb("#FAD7B2") : Color.FromArgb("#E5E7EB"),
            BorderWidth = 1,
            FontSize = 11,
            HeightRequest = 36,
            CornerRadius = 8
        };
        actionBtn.Clicked += async (s, e) =>
        {
            await Shell.Current.GoToAsync("//JournalPage");
        };
        Grid.SetColumn(actionBtn, 1);
        actionGrid.Children.Add(actionBtn);

        contentStack.Children.Add(actionGrid);

        mainGrid.Children.Add(contentStack);
        cardBorder.Content = mainGrid;

        return cardBorder;
    }

    private async void OnFilterParcelsClicked(object sender, EventArgs e)
    {
        var action = await DisplayActionSheet(
            "Filtrer les parcelles",
            "Annuler",
            null,
            "Toutes les parcelles",
            "Tomates",
            "Cacao",
            "Maïs",
            "En délai DAR uniquement",
            "Prêtes à récolter"
        );

        if (!string.IsNullOrWhiteSpace(action) && action != "Annuler")
        {
            _currentFilter = action;
            ApplyFilterAndRender();
        }
    }

    private async void OnOpenParcelBClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//JournalPage");
    }

    private async void OnOpenJournalClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//JournalPage");
    }

    private async void OnOpenCalculatorClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("CalculatorPage");
    }

    private async void OnCallAdvisorClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Conseiller Foumbot", "Contact de permanence : +237 670 11 22 33\nHoraires : 7h - 18h", "Appeler");
    }

    private async void OnNotificationsClicked(object sender, EventArgs e)
    {
        var darCount = _allParcels.Count(p => p.HasActiveDar && p.DarDaysRemaining > 0);
        var msg = darCount > 0
            ? $"1 Alerte prioritaire : Le Délai Avant Récolte (DAR) sur votre parcelle de tomates court jusqu'au 28 Octobre."
            : "Aucune alerte urgente. Vos parcelles sont en règle phytosanitaire.";
        await DisplayAlert("Notifications", msg, "Compris");
    }
}

