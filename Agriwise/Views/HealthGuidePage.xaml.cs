using Agriwise.Models;
using Agriwise.Services;

namespace Agriwise.Views;

public partial class HealthGuidePage : ContentPage
{
    private List<ChemicalProduct> _allProducts = new();
    private string _selectedCategory = "Tous";
    private string _searchQuery = string.Empty;

    public HealthGuidePage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await RefreshProductsAsync();
    }

    private async Task RefreshProductsAsync()
    {
        await DataService.Current.InitializeAsync();
        _allProducts = await DataService.Current.GetChemicalProductsAsync();
        LabelTabTous.Text = $"Tous ({_allProducts.Count})";
        FilterAndRenderProducts();
    }

    private void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        _searchQuery = e.NewTextValue ?? string.Empty;
        FilterAndRenderProducts();
    }

    #region Category Selection Handlers

    private void OnSelectCatTous(object sender, TappedEventArgs e) => SetCategory("Tous");
    private void OnSelectCatFongicides(object sender, TappedEventArgs e) => SetCategory("Fongicide");
    private void OnSelectCatInsecticides(object sender, TappedEventArgs e) => SetCategory("Insecticide");
    private void OnSelectCatHerbicides(object sender, TappedEventArgs e) => SetCategory("Herbicide");
    private void OnSelectCatBio(object sender, TappedEventArgs e) => SetCategory("Bio");

    private void SetCategory(string cat)
    {
        _selectedCategory = cat;

        HighlightTab(TabTous, LabelTabTous, cat == "Tous");
        HighlightTab(TabFongicides, LabelTabFongicides, cat == "Fongicide");
        HighlightTab(TabInsecticides, LabelTabInsecticides, cat == "Insecticide");
        HighlightTab(TabHerbicides, LabelTabHerbicides, cat == "Herbicide");
        HighlightTab(TabBio, LabelTabBio, cat == "Bio");

        FilterAndRenderProducts();
    }

    private void HighlightTab(Border tab, Label label, bool isSelected)
    {
        tab.BackgroundColor = isSelected ? Color.FromArgb("#013A13") : Colors.White;
        tab.Stroke = isSelected ? Colors.Transparent : Color.FromArgb("#E4E2E0");
        tab.StrokeThickness = isSelected ? 0 : 1;
        label.TextColor = isSelected ? Colors.White : Color.FromArgb("#414940");
        label.FontFamily = isSelected ? "OpenSansSemibold" : null;
    }

    #endregion

    private void FilterAndRenderProducts()
    {
        IEnumerable<ChemicalProduct> query = _allProducts;

        if (_selectedCategory != "Tous")
        {
            if (_selectedCategory == "Bio")
            {
                query = query.Where(p => p.Category.Contains("Bio", StringComparison.OrdinalIgnoreCase) || p.Name.Contains("Bio", StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                query = query.Where(p => p.Category.Contains(_selectedCategory, StringComparison.OrdinalIgnoreCase));
            }
        }

        if (!string.IsNullOrWhiteSpace(_searchQuery))
        {
            query = query.Where(p =>
                p.Name.Contains(_searchQuery, StringComparison.OrdinalIgnoreCase) ||
                p.ActiveSubstance.Contains(_searchQuery, StringComparison.OrdinalIgnoreCase) ||
                p.TargetPests.Contains(_searchQuery, StringComparison.OrdinalIgnoreCase) ||
                p.TargetCrops.Contains(_searchQuery, StringComparison.OrdinalIgnoreCase));
        }

        RenderProductCards(query.ToList());
    }

    private void RenderProductCards(List<ChemicalProduct> products)
    {
        ProductsContainer.Children.Clear();

        if (products.Count == 0)
        {
            ProductsContainer.Children.Add(new Label
            {
                Text = "Aucun intrant ne correspond à votre recherche.",
                TextColor = Color.FromArgb("#71796F"),
                FontSize = 13,
                HorizontalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 20)
            });
            return;
        }

        foreach (var p in products)
        {
            var card = CreateProductCard(p);
            ProductsContainer.Children.Add(card);
        }
    }

    private Border CreateProductCard(ChemicalProduct p)
    {
        var accentColor = Color.FromArgb(string.IsNullOrWhiteSpace(p.ColorHex) ? "#E67E22" : p.ColorHex);

        var card = new Border
        {
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(12) },
            BackgroundColor = Colors.White,
            Stroke = Color.FromArgb("#E4E2E0"),
            StrokeThickness = 1,
            Padding = new Thickness(0)
        };

        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = 5 },
                new ColumnDefinition { Width = GridLength.Star }
            }
        };

        // Left Stripe
        var stripe = new BoxView { Color = accentColor, VerticalOptions = LayoutOptions.Fill };
        Grid.SetColumn(stripe, 0);
        grid.Children.Add(stripe);

        // Content
        var stack = new VerticalStackLayout { Spacing = 8, Padding = new Thickness(14) };
        Grid.SetColumn(stack, 1);

        // Top Row: Category Pill + Homologué + DAR badge
        var topGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto }
            }
        };

        var leftMeta = new HorizontalStackLayout { Spacing = 6, VerticalOptions = LayoutOptions.Center };
        var catBorder = new Border
        {
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(8) },
            BackgroundColor = Color.FromArgb("#EFEEEC"),
            StrokeThickness = 0,
            Padding = new Thickness(6, 2)
        };
        catBorder.Content = new Label { Text = p.Category, FontSize = 10, TextColor = Color.FromArgb("#414940") };
        leftMeta.Children.Add(catBorder);

        if (p.IsCertifiedMinader)
        {
            leftMeta.Children.Add(new Label { Text = "Homologué", FontSize = 10, TextColor = Color.FromArgb("#1A6D0E"), VerticalOptions = LayoutOptions.Center });
        }
        Grid.SetColumn(leftMeta, 0);
        topGrid.Children.Add(leftMeta);

        // DAR pill
        var darBorder = new Border
        {
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(8) },
            BackgroundColor = p.DarDays > 0 ? Color.FromArgb("#FEF5EC") : Color.FromArgb("#EAF7EE"),
            Stroke = p.DarDays > 0 ? Color.FromArgb("#E67E22") : Color.FromArgb("#27AE60"),
            StrokeThickness = 1,
            Padding = new Thickness(6, 2)
        };
        var darStack = new HorizontalStackLayout { Spacing = 4, VerticalOptions = LayoutOptions.Center };
        darStack.Children.Add(new Image { Source = p.DarDays > 0 ? "ic_timer.png" : "ic_check.png", WidthRequest = 10, HeightRequest = 10, VerticalOptions = LayoutOptions.Center });
        darStack.Children.Add(new Label
        {
            Text = $"DAR : {p.DarDays} jour{(p.DarDays > 1 ? "s" : "")}",
            FontSize = 10,
            FontFamily = "OpenSansSemibold",
            TextColor = p.DarDays > 0 ? Color.FromArgb("#E67E22") : Color.FromArgb("#27AE60"),
            VerticalOptions = LayoutOptions.Center
        });
        darBorder.Content = darStack;
        Grid.SetColumn(darBorder, 1);
        topGrid.Children.Add(darBorder);

        stack.Children.Add(topGrid);

        // Title & Description
        stack.Children.Add(new Label
        {
            Text = p.Name,
            FontSize = 15,
            FontFamily = "OpenSansSemibold",
            TextColor = Color.FromArgb("#013A13")
        });

        stack.Children.Add(new Label
        {
            Text = $"{p.Instructions} (Cibles : {p.TargetPests})",
            FontSize = 11,
            TextColor = Color.FromArgb("#71796F")
        });

        // Bee Warning if applicable
        if (p.BeeWarning)
        {
            var beeBorder = new Border
            {
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(8) },
                BackgroundColor = Color.FromArgb("#FFDAD6"),
                StrokeThickness = 0,
                Padding = new Thickness(8, 4)
            };
            var beeStack = new HorizontalStackLayout { Spacing = 6, VerticalOptions = LayoutOptions.Center };
            beeStack.Children.Add(new Image { Source = "ic_alert.png", WidthRequest = 12, HeightRequest = 12, VerticalOptions = LayoutOptions.Center });
            beeStack.Children.Add(new Label
            {
                Text = "Avertissement Abeilles : Traiter uniquement tôt le matin ou après 17h.",
                FontSize = 10,
                TextColor = Color.FromArgb("#93000A"),
                FontFamily = "OpenSansSemibold",
                VerticalOptions = LayoutOptions.Center
            });
            beeBorder.Content = beeStack;
            stack.Children.Add(beeBorder);
        }

        // Bento Box: Dose + Toxicity
        var bentoGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Star }
            },
            BackgroundColor = Color.FromArgb("#F5F3F1"),
            Padding = new Thickness(10, 6)
        };

        var doseStack = new VerticalStackLayout { Spacing = 1 };
        doseStack.Children.Add(new Label { Text = "Dose Pulvé 15L :", FontSize = 10, TextColor = Color.FromArgb("#71796F") });
        doseStack.Children.Add(new Label
        {
            Text = $"{p.BaseDosePer15L:0.#} {p.Unit} / 15 Litres",
            FontSize = 11,
            FontFamily = "OpenSansSemibold",
            TextColor = Color.FromArgb("#013A13")
        });
        Grid.SetColumn(doseStack, 0);
        bentoGrid.Children.Add(doseStack);

        var toxStack = new VerticalStackLayout { Spacing = 1 };
        toxStack.Children.Add(new Label { Text = "Niveau toxicité :", FontSize = 10, TextColor = Color.FromArgb("#71796F") });
        toxStack.Children.Add(new Label
        {
            Text = p.ToxicityClass,
            FontSize = 11,
            FontFamily = "OpenSansSemibold",
            TextColor = accentColor
        });
        Grid.SetColumn(toxStack, 1);
        bentoGrid.Children.Add(toxStack);

        stack.Children.Add(bentoGrid);

        // Footer Actions Row: PPE info + "Doser sur parcelle" button
        var footerGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto }
            }
        };

        var ppeStack = new HorizontalStackLayout { Spacing = 4, VerticalOptions = LayoutOptions.Center };
        ppeStack.Children.Add(new Image { Source = "ic_mask.png", WidthRequest = 12, HeightRequest = 12, VerticalOptions = LayoutOptions.Center });
        ppeStack.Children.Add(new Label
        {
            Text = $"EPI : {p.PpeRequired}",
            FontSize = 10,
            TextColor = Color.FromArgb("#414940"),
            VerticalOptions = LayoutOptions.Center
        });
        Grid.SetColumn(ppeStack, 0);
        footerGrid.Children.Add(ppeStack);

        var doseBtn = new Button
        {
            Text = "Doser sur parcelle",
            BackgroundColor = Color.FromArgb("#F5F3F1"),
            TextColor = Color.FromArgb("#013A13"),
            FontSize = 11,
            HeightRequest = 32,
            CornerRadius = 6
        };
        doseBtn.Clicked += async (s, e) =>
        {
            await Shell.Current.GoToAsync("CalculatorPage");
        };
        Grid.SetColumn(doseBtn, 1);
        footerGrid.Children.Add(doseBtn);

        stack.Children.Add(footerGrid);

        grid.Children.Add(stack);
        card.Content = grid;

        return card;
    }

    private async void OnOpenCalculatorClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("CalculatorPage");
    }

    private async void OnEmergencyCallClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Urgence Intoxication", "Appel du SAMU Cameroun (119) ou Centre Anti-poison Yaoundé : +237 222 23 44 55", "Appeler");
    }
}

