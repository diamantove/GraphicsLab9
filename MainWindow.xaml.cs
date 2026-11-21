using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Microsoft.Win32;
using Lab9.Core;
using Lab9.Geometry;
using Lab9.Rendering;

namespace Lab9;

public partial class MainWindow : Window
{
    private readonly Renderer renderer = new();
    private Mesh mesh = ModelBuilder.Cube();
    private Texture texture = Texture.Checkerboard();
    private readonly Slider[] rotation;
    private readonly Slider[] translation;
    private readonly Slider[] scaling;
    private readonly Slider[] light;
    private readonly Slider[] color;
    private bool updating;

    public MainWindow()
    {
        InitializeComponent();
        rotation = AddParameters("Поворот, градусы", ["X", "Y", "Z"], -180, 180, [0, 0, 0]);
        translation = AddParameters("Перенос", ["X", "Y", "Z"], -3, 3, [0, 0, 0]);
        scaling = AddParameters("Масштаб", ["X", "Y", "Z"], 0.2, 2, [1, 1, 1]);
        light = AddParameters("Положение источника света", ["X", "Y", "Z"], -6, 6, [-3, 4, 5]);
        color = AddParameters("Цвет объекта", ["R", "G", "B"], 0, 255, [100, 170, 220]);
    }

    // Одинаковый код для всех групп: подпись, ползунок и текущее значение.
    private Slider[] AddParameters(string title, string[] labels, double minimum,
        double maximum, double[] defaults)
    {
        ParameterPanel.Children.Add(new TextBlock
        {
            Text = title,
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 12, 0, 4)
        });

        var sliders = new Slider[3];
        for (int i = 0; i < sliders.Length; i++)
        {
            var row = new Grid { Margin = new Thickness(0, 2, 0, 2) };
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(22) });
            row.ColumnDefinitions.Add(new ColumnDefinition());
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(48) });
            row.Children.Add(new TextBlock { Text = labels[i], VerticalAlignment = VerticalAlignment.Center });

            var slider = new Slider
            {
                Minimum = minimum,
                Maximum = maximum,
                Value = defaults[i],
                Tag = defaults[i],
                VerticalAlignment = VerticalAlignment.Center
            };
            slider.ValueChanged += Parameters_Changed;
            Grid.SetColumn(slider, 1);
            row.Children.Add(slider);
            sliders[i] = slider;

            var valueText = new TextBlock
            {
                TextAlignment = TextAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center
            };
            valueText.SetBinding(TextBlock.TextProperty, new Binding(nameof(Slider.Value))
            {
                Source = slider,
                StringFormat = "F2"
            });
            Grid.SetColumn(valueText, 2);
            row.Children.Add(valueText);
            ParameterPanel.Children.Add(row);
        }

        return sliders;
    }

    private static Vec3 ReadVector(Slider[] sliders) =>
        new(sliders[0].Value, sliders[1].Value, sliders[2].Value);

    private void Render()
    {
        // Размер берём у контейнера: Image без Source может иметь нулевой размер.
        // Иначе возникает замкнутый круг: нет размера — нет первого кадра — нет размера.
        if (!IsLoaded || updating || ViewportHost.ActualWidth < 1 || ViewportHost.ActualHeight < 1)
            return;

        Vec3 angles = ReadVector(rotation);
        Vec3 offset = ReadVector(translation);
        Vec3 scale = ReadVector(scaling);
        // Векторы-столбцы: сначала масштаб, затем повороты, затем перенос.
        Matrix4 model = Matrix4.Translation(offset.X, offset.Y, offset.Z)
            * Matrix4.Rotation(2, angles.Z)
            * Matrix4.Rotation(1, angles.Y)
            * Matrix4.Rotation(0, angles.X)
            * Matrix4.Scaling(scale.X, scale.Y, scale.Z);

        Viewport.Source = renderer.Render(mesh, model, ReadVector(light), ReadVector(color),
            (RenderMode)ModeBox.SelectedIndex, texture,
            (int)ViewportHost.ActualWidth, (int)ViewportHost.ActualHeight);
    }

    private void Window_Loaded(object sender, RoutedEventArgs e) => Render();
    private void Viewport_SizeChanged(object sender, SizeChangedEventArgs e) => Render();
    private void Settings_Changed(object sender, SelectionChangedEventArgs e) => Render();
    private void Parameters_Changed(object sender, RoutedPropertyChangedEventArgs<double> e) => Render();

    private void Model_Changed(object sender, SelectionChangedEventArgs e)
    {
        mesh = ModelBox.SelectedIndex switch
        {
            1 => ModelBuilder.Tetrahedron(),
            2 => ModelBuilder.Octahedron(),
            _ => ModelBuilder.Cube()
        };
        Render();
    }

    private void LoadTexture_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Выберите текстуру",
            Filter = "Изображения|*.png;*.jpg;*.jpeg;*.bmp"
        };
        if (dialog.ShowDialog() != true)
            return;

        try
        {
            texture = Texture.Load(dialog.FileName);
            TextureName.Text = Path.GetFileName(dialog.FileName);
            ModeBox.SelectedIndex = (int)RenderMode.Texture;
            Render();
        }
        catch (Exception exception) when (exception is IOException or NotSupportedException
            or ArgumentException or System.IO.FileFormatException or OverflowException
            or System.Runtime.InteropServices.COMException)
        {
            MessageBox.Show(this, "Не удалось загрузить текстуру.\n" + exception.Message,
                "Ошибка загрузки", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void Reset_Click(object sender, RoutedEventArgs e)
    {
        updating = true;
        foreach (Slider slider in rotation.Concat(translation).Concat(scaling).Concat(light).Concat(color))
            slider.Value = (double)slider.Tag;

        updating = false;
        Render();
    }
}
