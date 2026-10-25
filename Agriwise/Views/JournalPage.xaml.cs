using Agriwise.Models;
using Agriwise.Services;

namespace Agriwise.Views;

public partial class JournalPage : ContentPage
{
    private Parcel? _currentParcel;
    private List<CultureWork> _works = new();

    public JournalPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await RefreshJournalAsync();
    }

    private async Task RefreshJournalAsync()
    {
        await DataService.Current.InitializeAsync();
        
        // Default to parcel-b (Tomates) or first parcel
        _currentParcel = await DataService.Current.GetParcelByIdAsync("parcel-b");
        if (_currentParcel == null)
        {
            var parcels = await DataService.Current.GetParcelsAsync();
            _currentParcel = parcels.FirstOrDefault();
        }

        if (_currentParcel == null) return;

        // 1. Populate Parcel Info & Pills
        ParcelTitleLabel.Text = $"{_currentParcel.Name}";
        ParcelLocationLabel.Text = $"{_currentParcel.AreaHa:0.0} ha · {_currentParcel.Location}";
        ParcelAreaPillLabel.Text = $"{_currentParcel.AreaHa:0.0} Hectare";
        ParcelVarietyPillLabel.Text = string.IsNullOrWhiteSpace(_currentParcel.Variety) ? _currentParcel.Crop : _currentParcel.Variety;
        ParcelMoisturePillLabel.Text = $"Optimal ({_currentParcel.SoilMoisturePercent}%)";

        // 2. DAR Banner
        if (_currentParcel.HasActiveDar && _currentParcel.DarDaysRemaining > 0)
        {
            DarWarningBanner.IsVisible = true;
            DarCountdownDaysLabel.Text = _currentParcel.DarDaysRemaining.ToString();
            DarAuthorizedDateLabel.Text = (_currentParcel.DarHarvestAuthorizedDate ?? DateTime.Now.AddDays(11)).ToString("dd MMM yyyy");
            var chem = string.IsNullOrWhiteSpace(_currentParcel.LastChemicalApplied) ? "phytosanitaire" : _currentParcel.LastChemicalApplied;
            DarSubtitleLabel.Text = $"Traitement {chem} récent détecté";
        }
        else
        {
            DarWarningBanner.Stroke = Color.FromArgb("#86EFAC");
            DarCountdownDaysLabel.Text = "0";
            DarAuthorizedDateLabel.Text = "Dès aujourd'hui";
            DarSubtitleLabel.Text = "Aucune rémanence active · Récolte sécurisée";
        }

        // 3. Load Operations Timeline
        _works = await DataService.Current.GetCultureWorksAsync(_currentParcel.Id);
        OperationsCountLabel.Text = $"{_works.Count} opérations";

        var totalCost = _works.Sum(w => w.CostFCFA);
        TotalExpensesLabel.Text = $"{totalCost:N0} FCFA".Replace(",", " ");

        RenderTimeline();
    }

    private void RenderTimeline()
    {
        TimelineContainer.Children.Clear();

        if (_works.Count == 0)
        {
            TimelineContainer.Children.Add(new Label
            {
                Text = "Aucune intervention enregistrée sur cette parcelle.",
                TextColor = Color.FromArgb("#71796F"),
                FontSize = 13,
                HorizontalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 16)
            });
            return;
        }

        foreach (var work in _works)
        {
            var card = CreateWorkCard(work);
            TimelineContainer.Children.Add(card);
        }
    }

    private Border CreateWorkCard(CultureWork work)
    {
        var hasDar = !string.IsNullOrWhiteSpace(work.DarBadgeText);
        var borderStroke = hasDar ? Color.FromArgb("#FAD7B2") : Color.FromArgb("#E5E7EB");

        var card = new Border
        {
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(16) },
            BackgroundColor = Colors.White,
            Stroke = borderStroke,
            StrokeThickness = 1,
            Padding = new Thickness(14)
        };

        var stack = new VerticalStackLayout { Spacing = 8 };

        // Top Row: Category + Title + Badge/Cost
        var topGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto }
            }
        };

        var titleStack = new VerticalStackLayout { Spacing = 2 };
        titleStack.Children.Add(new Label
        {
            Text = work.Category.ToUpperInvariant(),
            FontSize = 10,
            FontFamily = "OpenSansSemibold",
            TextColor = Color.FromArgb(string.IsNullOrWhiteSpace(work.CategoryColorHex) ? "#1A6D0E" : work.CategoryColorHex)
        });
        titleStack.Children.Add(new Label
        {
            Text = $"{work.Date:dd MMMM} · {work.Title}",
            FontSize = 14,
            FontFamily = "OpenSansSemibold",
            TextColor = Color.FromArgb("#013A13")
        });
        Grid.SetColumn(titleStack, 0);
        topGrid.Children.Add(titleStack);

        if (hasDar)
        {
            var darPill = new Border
            {
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(8) },
                BackgroundColor = Color.FromArgb("#FEF5EC"),
                Stroke = Color.FromArgb("#E67E22"),
                StrokeThickness = 1,
                Padding = new Thickness(8, 3)
            };
            darPill.Content = new Label
            {
                Text = work.DarBadgeText,
                FontSize = 10,
                FontFamily = "OpenSansSemibold",
                TextColor = Color.FromArgb("#9C4400")
            };
            Grid.SetColumn(darPill, 1);
            topGrid.Children.Add(darPill);
        }
        else if (work.CostFCFA > 0)
        {
            var costLbl = new Label
            {
                Text = $"{work.CostFCFA:N0} F".Replace(",", " "),
                FontSize = 14,
                FontFamily = "OpenSansSemibold",
                TextColor = Color.FromArgb("#013A13"),
                VerticalOptions = LayoutOptions.Center
            };
            Grid.SetColumn(costLbl, 1);
            topGrid.Children.Add(costLbl);
        }

        stack.Children.Add(topGrid);

        // Product details if any
        if (!string.IsNullOrWhiteSpace(work.Product))
        {
            var prodBorder = new Border
            {
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(10) },
                BackgroundColor = Color.FromArgb("#F8F9FA"),
                Stroke = Color.FromArgb("#E5E7EB"),
                StrokeThickness = 1,
                Padding = new Thickness(10, 8)
            };
            var prodStack = new VerticalStackLayout { Spacing = 4 };
            
            var detailGrid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Star }
                }
            };
            detailGrid.Children.Add(new Label
            {
                Text = $"Produit : {work.Product}",
                FontSize = 11,
                TextColor = Color.FromArgb("#013A13")
            });
            if (!string.IsNullOrWhiteSpace(work.DoseApplied))
            {
                var doseLbl = new Label
                {
                    Text = $"Dose : {work.DoseApplied}",
                    FontSize = 11,
                    TextColor = Color.FromArgb("#013A13")
                };
                Grid.SetColumn(doseLbl, 1);
                detailGrid.Children.Add(doseLbl);
            }
            prodStack.Children.Add(detailGrid);

            if (work.IsDoseCompliant)
            {
                var compliantRow = new HorizontalStackLayout { Spacing = 4, VerticalOptions = LayoutOptions.Center };
                compliantRow.Children.Add(new Image { Source = "ic_check.png", WidthRequest = 12, HeightRequest = 12, VerticalOptions = LayoutOptions.Center });
                compliantRow.Children.Add(new Label
                {
                    Text = "Dose recommandée respectée",
                    FontSize = 11,
                    TextColor = Color.FromArgb("#1A6D0E"),
                    FontFamily = "OpenSansSemibold",
                    VerticalOptions = LayoutOptions.Center
                });
                prodStack.Children.Add(compliantRow);
            }

            prodBorder.Content = prodStack;
            stack.Children.Add(prodBorder);
        }

        // Voice note box if any
        if (!string.IsNullOrWhiteSpace(work.AudioNoteText))
        {
            var noteBorder = new Border
            {
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(10) },
                BackgroundColor = Color.FromArgb("#F8F9FA"),
                Stroke = Color.FromArgb("#E5E7EB"),
                StrokeThickness = 1,
                Padding = new Thickness(10, 8)
            };
            var noteGrid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto }
                },
                ColumnSpacing = 8
            };

            var micImg = new Image { Source = "ic_mic.png", WidthRequest = 16, HeightRequest = 16, VerticalOptions = LayoutOptions.Center };
            Grid.SetColumn(micImg, 0);
            noteGrid.Children.Add(micImg);

            var noteLbl = new Label
            {
                Text = $"Note audio : « {work.AudioNoteText} »",
                FontSize = 11,
                TextColor = Color.FromArgb("#414940"),
                FontAttributes = FontAttributes.Italic,
                VerticalOptions = LayoutOptions.Center
            };
            Grid.SetColumn(noteLbl, 1);
            noteGrid.Children.Add(noteLbl);

            var playBtn = new Border
            {
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(14) },
                BackgroundColor = Color.FromArgb("#013A13"),
                StrokeThickness = 0,
                WidthRequest = 30,
                HeightRequest = 30
            };
            playBtn.Content = new Image { Source = "ic_play.png", WidthRequest = 12, HeightRequest = 12, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
            var tap = new TapGestureRecognizer();
            tap.Tapped += async (s, e) =>
            {
                await DisplayAlert("Lecture Note Vocale", $"« {work.AudioNoteText} »\n\nEnregistré le {work.Date:dd/MM/yyyy à HH:mm} par le planteur.", "Fermer");
            };
            playBtn.GestureRecognizers.Add(tap);
            Grid.SetColumn(playBtn, 2);
            noteGrid.Children.Add(playBtn);

            noteBorder.Content = noteGrid;
            stack.Children.Add(noteBorder);
        }

        card.Content = stack;
        return card;
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//DashboardPage");
    }

    private async void OnAddWorkClicked(object sender, EventArgs e)
    {
        var action = await DisplayActionSheet(
            "Ajouter une intervention",
            "Annuler",
            null,
            "Calculer & consigner un traitement (Calculateur)",
            "Fertilisation (NPK / Urée)",
            "Sarclage / Entretien manuel",
            "Irrigation / Arrosage",
            "Enregistrer note audio au champ"
        );

        if (string.IsNullOrWhiteSpace(action) || action == "Annuler") return;

        if (action.Contains("Calculateur"))
        {
            await Shell.Current.GoToAsync("CalculatorPage");
            return;
        }

        if (action.Contains("note audio"))
        {
            var note = await DisplayPromptAsync("Note vocale simulée", "Dictée du champ :", "Consigner", "Annuler", "Ex: Pulvérisation finie sous ciel couvert sans vent");
            if (!string.IsNullOrWhiteSpace(note) && _currentParcel != null)
            {
                var work = new CultureWork
                {
                    ParcelId = _currentParcel.Id,
                    Title = "Observation au champ",
                    Category = "Entretien",
                    Date = DateTime.Now,
                    AudioNoteText = note,
                    CostFCFA = 0,
                    CategoryColorHex = "#71796F"
                };
                await DataService.Current.AddCultureWorkAsync(work);
                await RefreshJournalAsync();
                await DisplayAlert("Succès", "Note consignée au carnet de culture.", "OK");
            }
            return;
        }

        // Generic Task
        string costStr = await DisplayPromptAsync(action, "Coût engagé en FCFA (optionnel) :", "Valider", "Annuler", "Ex: 15000", keyboard: Keyboard.Numeric);
        if (costStr == null) return; // cancelled

        int.TryParse(costStr, out int cost);
        if (_currentParcel != null)
        {
            var work = new CultureWork
            {
                ParcelId = _currentParcel.Id,
                Title = action,
                Category = action.Contains("Fertilisation") ? "Fertilisation" : "Entretien",
                Date = DateTime.Now,
                CostFCFA = cost,
                CategoryColorHex = action.Contains("Fertilisation") ? "#1A6D0E" : "#71796F"
            };
            await DataService.Current.AddCultureWorkAsync(work);
            await RefreshJournalAsync();
            await DisplayAlert("Succès", $"{action} enregistrée au carnet de culture.", "OK");
        }
    }

    private async void OnOpenFinancesClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//FinancesPage");
    }
}

