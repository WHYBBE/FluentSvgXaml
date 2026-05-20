using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;

namespace FluentSvgXaml.Controls;

public static class WatermarkService
{
    public static readonly DependencyProperty WatermarkProperty = DependencyProperty.RegisterAttached(
       "Watermark",
       typeof(object),
       typeof(WatermarkService),
       new FrameworkPropertyMetadata(null, OnWatermarkChanged));

    private static readonly Dictionary<object, ItemsControl> _itemsControls = [];

    public static object GetWatermark(DependencyObject d) => d.GetValue(WatermarkProperty);

    public static void SetWatermark(DependencyObject d, object value) => d.SetValue(WatermarkProperty, value);

    private static void OnWatermarkChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Control control)
            return;

        if (e.OldValue is not null)
        {
            control.Loaded -= Control_Loaded;
            control.Unloaded -= Control_Unloaded;
            control.GotKeyboardFocus -= Control_GotKeyboardFocus;
            control.LostKeyboardFocus -= Control_Loaded;

            RemoveWatermark(control);

            if (d is ItemsControl and not ComboBox)
            {
                var itemsControl = (ItemsControl)d;
                var generator = itemsControl.ItemContainerGenerator;
                generator.ItemsChanged -= ItemsChanged;
                _itemsControls.Remove(generator);

                var prop = DependencyPropertyDescriptor.FromProperty(ItemsControl.ItemsSourceProperty, itemsControl.GetType());
                prop.RemoveValueChanged(itemsControl, ItemsSourceChanged);
            }
        }

        if (e.NewValue is null)
            return;

        control.Loaded += Control_Loaded;
        control.Unloaded += Control_Unloaded;

        if (d is ComboBox or TextBox)
        {
            control.GotKeyboardFocus += Control_GotKeyboardFocus;
            control.LostKeyboardFocus += Control_Loaded;
        }

        if (d is ItemsControl and not ComboBox)
        {
            var itemsControl = (ItemsControl)d;
            var generator = itemsControl.ItemContainerGenerator;
            generator.ItemsChanged += ItemsChanged;
            _itemsControls[generator] = itemsControl;

            var prop = DependencyPropertyDescriptor.FromProperty(ItemsControl.ItemsSourceProperty, itemsControl.GetType());
            prop.AddValueChanged(itemsControl, ItemsSourceChanged);
        }
    }

    private static void Control_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is Control control && ShouldShowWatermark(control))
            ShowWatermark(control);
    }

    private static void Control_Unloaded(object sender, RoutedEventArgs e)
    {
        if (sender is Control control)
            RemoveWatermark(control);
    }

    private static void Control_GotKeyboardFocus(object sender, RoutedEventArgs e)
    {
        if (sender is Control c && !ShouldShowWatermark(c))
            RemoveWatermark(c);
    }

    private static void ItemsSourceChanged(object? sender, EventArgs e)
    {
        if (sender is not ItemsControl c)
            return;

        if (c.ItemsSource != null && !ShouldShowWatermark(c))
            RemoveWatermark(c);
        else
            ShowWatermark(c);
    }

    private static void ItemsChanged(object sender, ItemsChangedEventArgs e)
    {
        if (_itemsControls.TryGetValue(sender, out var control))
        {
            if (ShouldShowWatermark(control))
                ShowWatermark(control);
            else
                RemoveWatermark(control);
        }
    }

    private static void RemoveWatermark(UIElement control)
    {
        var layer = AdornerLayer.GetAdornerLayer(control);
        if (layer is null)
            return;

        var adorners = layer.GetAdorners(control);
        if (adorners is null)
            return;

        foreach (var adorner in adorners)
        {
            if (adorner is WatermarkAdorner)
            {
                layer.Remove(adorner);
            }
        }
    }

    private static void ShowWatermark(Control control)
    {
        var layer = AdornerLayer.GetAdornerLayer(control);
        if (layer is null)
            return;

        RemoveWatermark(control);
        layer.Add(new WatermarkAdorner(control, GetWatermark(control)));
    }

    private static bool ShouldShowWatermark(Control c)
    {
        return c switch
        {
            ComboBox comboBox => comboBox.Text == string.Empty,
            TextBoxBase => (c as TextBox)?.Text == string.Empty,
            ItemsControl itemsControl => itemsControl.Items.Count == 0,
            _ => false
        };
    }
}
