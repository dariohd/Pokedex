using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Pokedex.Core.Models;
using Pokedex.Wpf.Services;

namespace Pokedex.Wpf.Controls;

public class EvolutionTreeView : UserControl
{
    public static readonly DependencyProperty TreeProperty =
        DependencyProperty.Register(nameof(Tree), typeof(EvolutionNode), typeof(EvolutionTreeView),
            new PropertyMetadata(null, OnTreeChanged));

    public static readonly DependencyProperty CurrentPokemonIdProperty =
        DependencyProperty.Register(nameof(CurrentPokemonId), typeof(int), typeof(EvolutionTreeView),
            new PropertyMetadata(0, OnTreeChanged));

    public static readonly DependencyProperty ShinyProperty =
        DependencyProperty.Register(nameof(Shiny), typeof(bool), typeof(EvolutionTreeView),
            new PropertyMetadata(false, OnTreeChanged));

    public EvolutionNode? Tree
    {
        get => (EvolutionNode?)GetValue(TreeProperty);
        set => SetValue(TreeProperty, value);
    }

    public int CurrentPokemonId
    {
        get => (int)GetValue(CurrentPokemonIdProperty);
        set => SetValue(CurrentPokemonIdProperty, value);
    }

    public bool Shiny
    {
        get => (bool)GetValue(ShinyProperty);
        set => SetValue(ShinyProperty, value);
    }

    private readonly ScrollViewer _scroll = new() { HorizontalScrollBarVisibility = ScrollBarVisibility.Auto };
    private readonly StackPanel _root = new() { Orientation = Orientation.Horizontal, Margin = new Thickness(16) };

    public EvolutionTreeView()
    {
        _scroll.Content = _root;
        Content = _scroll;
    }

    private static void OnTreeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is EvolutionTreeView view)
            view.Rebuild();
    }

    private void Rebuild()
    {
        _root.Children.Clear();
        if (Tree == null)
        {
            _root.Children.Add(new TextBlock
            {
                Text = "Aucune donnée d'évolution.",
                Foreground = Brushes.Gray,
                Margin = new Thickness(8)
            });
            return;
        }

        if (Tree.Children.Count == 0)
        {
            _root.Children.Add(new TextBlock
            {
                Text = "Ce Pokémon est au sommet de sa lignée évolutive.",
                Foreground = Brushes.Gray,
                Margin = new Thickness(8)
            });
            return;
        }

        _root.Children.Add(BuildNodePanel(Tree, isRoot: true));
    }

    private UIElement BuildNodePanel(EvolutionNode node, bool isRoot)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal };

        if (!isRoot)
            panel.Children.Add(MakeArrow());

        panel.Children.Add(MakeCard(node));

        if (node.Children.Count == 1)
        {
            panel.Children.Add(MakeArrow());
            panel.Children.Add(BuildNodePanel(node.Children[0], isRoot: false));
            return panel;
        }

        if (node.Children.Count > 1)
        {
            panel.Children.Add(MakeArrow());
            var branch = new StackPanel();
            foreach (var child in node.Children)
                branch.Children.Add(BuildNodePanel(child, isRoot: false));
            panel.Children.Add(branch);
        }

        return panel;
    }

    private Border MakeCard(EvolutionNode node)
    {
        var isCurrent = node.Id == CurrentPokemonId;
        var border = new Border
        {
            Width = 140,
            Margin = new Thickness(6),
            Padding = new Thickness(8),
            CornerRadius = new CornerRadius(10),
            BorderBrush = isCurrent ? new SolidColorBrush(Color.FromRgb(220, 53, 69)) : Brushes.LightGray,
            BorderThickness = new Thickness(isCurrent ? 2 : 1),
            Background = isCurrent ? new SolidColorBrush(Color.FromRgb(255, 235, 238)) : Brushes.White
        };

        var stack = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center };

        var img = new Image { Width = 96, Height = 96, Stretch = Stretch.Uniform };
        _ = LoadImageAsync(img, node.Id);

        stack.Children.Add(img);
        stack.Children.Add(new TextBlock
        {
            Text = node.DisplayName,
            FontWeight = FontWeights.Bold,
            TextAlignment = TextAlignment.Center,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 6, 0, 0)
        });

        if (!string.IsNullOrEmpty(node.Trigger))
        {
            stack.Children.Add(new TextBlock
            {
                Text = node.Trigger,
                FontSize = 11,
                Foreground = Brushes.Gray,
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            });
        }

        border.Child = stack;
        return border;
    }

    private static TextBlock MakeArrow() => new()
    {
        Text = "→",
        FontSize = 22,
        FontWeight = FontWeights.Bold,
        Foreground = new SolidColorBrush(Color.FromRgb(220, 53, 69)),
        VerticalAlignment = VerticalAlignment.Center,
        Margin = new Thickness(4, 40, 4, 0)
    };

    private async Task LoadImageAsync(Image target, int id)
    {
        try
        {
            using var client = new HttpClient();
            var service = new PokemonImageService(client);
            var bmp = await service.LoadSpriteAsync(id, Shiny);
            if (bmp != null)
                target.Source = bmp;
        }
        catch { /* ignore */ }
    }
}
