using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;

public class NoesisMenu
{
    [UnityEditor.MenuItem("Tools/NoesisGUI/About NoesisGUI...", false, 30000)]
    static void OpenAbout()
    {
        EditorWindow.GetWindow(typeof(NoesisAbout), true, "About NoesisGUI");
    }

    [UnityEditor.MenuItem("Tools/NoesisGUI/Settings...", false, 30050)]
    static void OpenSettings()
    {
        Selection.activeObject = NoesisSettings.Get();
    }

    [UnityEditor.MenuItem("Tools/NoesisGUI/Welcome Screen...", false, 30100)]
    static void OpenWelcome()
    {
        NoesisWelcome.Open();
    }

    [UnityEditor.MenuItem("Tools/NoesisGUI/Documentation", false, 30103)]
    static void OpenDocumentation()
    {
        string docPath = Path.GetFullPath("Packages/com.noesis.noesisgui/Documentation~/Documentation.html");

        if (File.Exists(docPath))
        {
            UnityEngine.Application.OpenURL("file://" + docPath.Replace(" ", "%20"));
        }
        else
        {
            UnityEngine.Application.OpenURL("http://www.noesisengine.com/docs");
        }
    }

    [UnityEditor.MenuItem("Tools/NoesisGUI/Forums", false, 30104)]
    static void OpenForum()
    {
        UnityEngine.Application.OpenURL("http://forums.noesisengine.com/");
    }

    [UnityEditor.MenuItem("Tools/NoesisGUI/Review...", false, 30105)]
    static void OpenReview()
    {
        EditorWindow.GetWindow(typeof(NoesisReview), true, "Support our development");
    }

    [UnityEditor.MenuItem("Tools/NoesisGUI/Release Notes", false, 30150)]
    static public void OpenReleaseNotes()
    {
        string docPath = Application.dataPath + "/../NoesisDoc/Doc/Gui.Core.Changelog.html";

        if (File.Exists(docPath))
        {
            UnityEngine.Application.OpenURL("file://" + docPath.Replace(" ", "%20"));
        }
        else
        {
            UnityEngine.Application.OpenURL("http://www.noesisengine.com/docs/Gui.Core.Changelog.html");
        }
    }

    [UnityEditor.MenuItem("Tools/NoesisGUI/Report a bug", false, 30151)]
    static void OpenReportBug()
    {
        UnityEngine.Application.OpenURL("http://bugs.noesisengine.com/");
    }

    [UnityEditor.MenuItem("Assets/Create/NoesisGUI/Render Texture", false, 350)]
    [UnityEditor.MenuItem("Tools/NoesisGUI/Create/Render Texture", false, 30150)]
    static void CreateNoesisRenderTexture()
    {
        // Render textures created by Unity editor always have sRGB property set to false and are
        // not compatible with linear color space. Creating them by code allow us to set readWrite
        // to Default and be compatible with both linear and gamma color space
        RenderTexture surface = new RenderTexture(256, 256, 24);
        ProjectWindowUtil.CreateAsset(surface, "New Render Texture.renderTexture");
    }

    internal static Object CreateScriptAssetWithContent(string pathName, string templateContent)
    {
        string fullPath = Path.GetFullPath(pathName);
        File.WriteAllText(fullPath, templateContent);

        AssetDatabase.ImportAsset(pathName);
        return AssetDatabase.LoadAssetAtPath(pathName, typeof(Object));
    }

    internal class DoCreateAssetWithContent : EndNameEditAction
    {
        public string filecontent;

        public override void Action(int instanceId, string pathName, string resourceFile)
        {
            Object o = CreateScriptAssetWithContent(pathName, filecontent);
            ProjectWindowUtil.ShowCreatedAsset(o);
        }
    }

    private static void CreateXaml(string content)
    {
        var action = ScriptableObject.CreateInstance<DoCreateAssetWithContent>();
        action.filecontent = content;

        Texture2D icon = (Texture2D)AssetDatabase.LoadAssetAtPath("Packages/com.noesis.noesisgui/Editor/icon.png", typeof(Texture2D));
        ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, action, "New Xaml.xaml", icon, null);
    }

    [UnityEditor.MenuItem("Assets/Create/NoesisGUI/XAML - Hello World", false, 230)]
    [UnityEditor.MenuItem("Tools/NoesisGUI/Create/XAML - Hello World", false, 30061)]
    static void CreateXamlHelloWorld()
    {
        CreateXaml
        (
          System.String.Join(System.Environment.NewLine,
            "<Grid xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">",
            "  <TextBlock FontWeight=\"Bold\" FontSize=\"40pt\" Foreground=\"DodgerBlue\"",
            "    HorizontalAlignment=\"Center\" VerticalAlignment=\"Center\">",
            "    Hello World!",
            "  </TextBlock>",
            "</Grid>"
            )
        );
    }

    [UnityEditor.MenuItem("Assets/Create/NoesisGUI/XAML - Linear Gradient", false, 231)]
    [UnityEditor.MenuItem("Tools/NoesisGUI/Create/XAML - Linear Gradient", false, 30062)]
    static void CreateXamlLinearGradient()
    {
        CreateXaml
        (
          System.String.Join(System.Environment.NewLine,
            "<Grid xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">",
            "  <Ellipse Width=\"300\" Height=\"300\" Stroke=\"#80FFFF00\" StrokeThickness=\"15\">",
            "    <Ellipse.Fill>",
            "      <LinearGradientBrush StartPoint=\"0,0\" EndPoint=\"1,1\">",
            "        <GradientStop Offset=\"0\" Color=\"Gold\"/>",
            "        <GradientStop Offset=\"1\" Color=\"DarkOrange\"/>",
            "      </LinearGradientBrush>",
            "    </Ellipse.Fill>",
            "  </Ellipse>",
            "</Grid>"
          )
        );
    }

    [UnityEditor.MenuItem("Assets/Create/NoesisGUI/XAML - Layout Grid", false, 232)]
    [UnityEditor.MenuItem("Tools/NoesisGUI/Create/XAML - Layout Grid", false, 30063)]
    static void CreateXamlLayoutGrid()
    {
        CreateXaml
        (
          System.String.Join(System.Environment.NewLine,
            "<Grid xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">",
            "  <Grid.ColumnDefinitions>",
            "    <ColumnDefinition Width=\"100\"/>",
            "    <ColumnDefinition Width=\"*\"/>",
            "  </Grid.ColumnDefinitions>",
            "  <Grid.RowDefinitions>",
            "    <RowDefinition Height=\"50\"/>",
            "    <RowDefinition Height=\"*\"/>",
            "    <RowDefinition Height=\"50\"/>",
            "  </Grid.RowDefinitions>",
            "  <Rectangle Grid.Row=\"0\" Grid.Column=\"0\" Grid.ColumnSpan=\"2\" Fill=\"YellowGreen\"/>",
            "  <Rectangle Grid.Row=\"1\" Grid.Column=\"0\" Fill=\"Gray\"/>",
            "  <Rectangle Grid.Row=\"1\" Grid.Column=\"1\" Fill=\"Silver\"/>",
            "  <Rectangle Grid.Row=\"2\" Grid.Column=\"0\" Grid.ColumnSpan=\"2\" Fill=\"Orange\"/>",
            "</Grid>"
          )
        );
    }

    [UnityEditor.MenuItem("Assets/Create/NoesisGUI/XAML - Layout Canvas", false, 233)]
    [UnityEditor.MenuItem("Tools/NoesisGUI/Create/XAML - Layout Canvas", false, 30064)]
    static void CreateXamlLayoutCanvas()
    {
        CreateXaml
        (
          System.String.Join(System.Environment.NewLine,
            "<Canvas xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">",
            "  <Ellipse Canvas.Left=\"5\" Canvas.Top=\"5\" Width=\"300\" Height=\"300\" Fill=\"YellowGreen\"/>",
            "  <Ellipse Canvas.Left=\"35\" Canvas.Top=\"35\" Width=\"200\" Height=\"200\" Fill=\"Gold\"/>",
            "  <Ellipse Canvas.Left=\"55\" Canvas.Top=\"55\" Width=\"100\" Height=\"100\" Fill=\"Orange\"/>",
            "</Canvas>"
          )
        );
    }

    [UnityEditor.MenuItem("Assets/Create/NoesisGUI/XAML - Button Template", false, 234)]
    [UnityEditor.MenuItem("Tools/NoesisGUI/Create/XAML - Button Template", false, 30065)]
    static void CreateXamlButtonTemplate()
    {
        CreateXaml
        (
          System.String.Join(System.Environment.NewLine,
            "<StackPanel xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"",
            "  xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\">",
            "  <StackPanel.Resources>",
            "    <Style TargetType=\"{x:Type Button}\">",
            "      <Setter Property=\"Template\">",
            "        <Setter.Value>",
            "          <ControlTemplate TargetType=\"{x:Type Button}\">",
            "            <Border x:Name=\"Border\" Background=\"YellowGreen\" CornerRadius=\"4\">",
            "              <ContentPresenter VerticalAlignment=\"Center\" HorizontalAlignment=\"Center\"/>",
            "            </Border>",
            "            <ControlTemplate.Triggers>",
            "              <Trigger Property=\"IsMouseOver\" Value=\"True\">",
            "                <Setter TargetName=\"Border\" Property=\"Background\" Value=\"Gold\"/>",
            "              </Trigger>",
            "              <Trigger Property=\"IsPressed\" Value=\"True\">",
            "                <Setter TargetName=\"Border\" Property=\"Background\" Value=\"Orange\"/>",
            "              </Trigger>",
            "            </ControlTemplate.Triggers>",
            "          </ControlTemplate>",
            "        </Setter.Value>",
            "      </Setter>",
            "      <Setter Property=\"FontFamily\" Value=\"Arial\"/>",
            "    </Style>",
            "  </StackPanel.Resources>",
            "  <Button Width=\"100\" Height=\"30\" Margin=\"5\" Content=\"Button A\"/>",
            "  <Button Width=\"100\" Height=\"30\" Margin=\"5\" Content=\"Button B\"/>",
            "</StackPanel>"
          )
        );
    }

    [UnityEditor.MenuItem("Assets/Create/NoesisGUI/XAML - DataBinding Text", false, 235)]
    [UnityEditor.MenuItem("Tools/NoesisGUI/Create/XAML - DataBinding Text", false, 30066)]
    static void CreateXamlDataBindingText()
    {
        CreateXaml
        (
          System.String.Join(System.Environment.NewLine,
            "<StackPanel xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"",
            "  xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"",
            "  HorizontalAlignment=\"Center\" VerticalAlignment=\"Center\" Width=\"400\">",
            "  <TextBlock Text=\"{Binding ElementName=MyTextBox, Path=Text}\" FontSize=\"45\"/>",
            "  <TextBox x:Name=\"MyTextBox\">Type here...</TextBox>",
            "</StackPanel>"
          )
        );
    }

    [UnityEditor.MenuItem("Assets/Create/NoesisGUI/XAML - DataBinding Slider", false, 236)]
    [UnityEditor.MenuItem("Tools/NoesisGUI/Create/XAML - DataBinding Slider", false, 30067)]
    static void CreateXamlDataBindingSlider()
    {
        CreateXaml
        (
          System.String.Join(System.Environment.NewLine,
            "<Grid xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"",
            "  xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"",
            "  Width=\"300\" Height=\"300\" Background=\"DimGray\">",
            "  <Slider x:Name=\"MySlider\" VerticalAlignment=\"Top\"",
            "    Minimum=\"10\" Maximum=\"200\" Value=\"60\" Margin=\"10\"/>",
            "  <Rectangle Fill=\"Orange\" VerticalAlignment=\"Center\" HorizontalAlignment=\"Center\"",
            "    Width=\"{Binding ElementName=MySlider, Path=Value}\" Height=\"{Binding ElementName=MySlider, Path=Value}\"/>",
            "</Grid>"
          )
        );
    }

    [UnityEditor.MenuItem("Assets/Create/NoesisGUI/XAML - DropShadow", false, 237)]
    [UnityEditor.MenuItem("Tools/NoesisGUI/Create/XAML - DropShadow", false, 30068)]
    static void CreateXamlDropShadow()
    {
        CreateXaml
        (
          System.String.Join(System.Environment.NewLine,
            "<Grid xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">",
            "  <Rectangle Width=\"300\" Height=\"300\" Fill=\"DodgerBlue\">",
            "    <Rectangle.Effect>",
            "      <DropShadowEffect BlurRadius=\"10\" ShadowDepth=\"20\" Opacity=\"0.7\"/>",
            "    </Rectangle.Effect>",
            "  </Rectangle>",
            "</Grid>"
          )
        );
    }

    internal class DoCreateUserControl : EndNameEditAction
    {
        public override void Action(int instanceId, string pathName, string resourceFile)
        {
            var xaml = System.String.Join(System.Environment.NewLine,
              "<UserControl x:Class=\"Testing.%NAME%\"",
              "  xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"",
              "  xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"",
              "  x:Name=\"Root\">",
              "  <StackPanel Orientation=\"Horizontal\" HorizontalAlignment=\"Center\" VerticalAlignment=\"Center\">",
              "    <Button Content=\"Click me\" Click=\"Button_Click\"/>",
              "    <TextBlock Text=\"{Binding Counter, ElementName=Root, StringFormat='Button clicked {0} time(s)'}\" Margin=\"5,0,0,0\" VerticalAlignment=\"Center\"/>",
              "  </StackPanel>",
              "</UserControl>"
            )
            .Replace("%NAME%", Path.GetFileNameWithoutExtension(pathName));

            var code = System.String.Join(System.Environment.NewLine,
              "#if UNITY_5_3_OR_NEWER",
              "    #define NOESIS",
              "    using Noesis;",
              "#else",
              "    using System.Windows;",
              "    using System.Windows.Controls;",
              "#endif",
              "",
              "namespace Testing",
              "{",
              "    public partial class %NAME%: UserControl",
              "    {",
              "        public %NAME%()",
              "        {",
              "            InitializeComponent();",
              "        }",
              "",
              "        public int Counter",
              "        {",
              "            get { return (int)GetValue(CounterProperty); }",
              "            set { SetValue(CounterProperty, value); }",
              "        }",
              "",
              "        public static readonly DependencyProperty CounterProperty =  DependencyProperty.Register(",
              "            \"Counter\", typeof(int), typeof(%NAME%), new PropertyMetadata(0));",
              "",
              "    #if NOESIS",
              "        protected override bool ConnectEvent(object source, string eventName, string handlerName)",
              "        {",
              "            if (eventName == \"Click\" && handlerName == \"Button_Click\")",
              "            {",
              "                ((Button)source).Click += this.Button_Click;",
              "                return true;",
              "            }",
              "",
              "            return false;",
              "        }",
              "",
              "        private void InitializeComponent()",
              "        {",
              "            NoesisUnity.LoadComponent(this);",
              "        }",
              "    #endif",
              "",
              "        private void Button_Click(object sender, RoutedEventArgs args)",
              "        {",
              "            Counter++;",
              "        }",
              "    };",
              "}"
            )
            .Replace("%NAME%", Path.GetFileNameWithoutExtension(pathName));

            CreateScriptAssetWithContent(pathName + ".cs", code);

            Object o = CreateScriptAssetWithContent(pathName, xaml);
            ProjectWindowUtil.ShowCreatedAsset(o);
        }
    }

    [UnityEditor.MenuItem("Assets/Create/NoesisGUI/UserControl", false, 320)]
    [UnityEditor.MenuItem("Tools/NoesisGUI/Create/UserControl", false, 30100)]
    static void CreateUserControl()
    {
        var action = ScriptableObject.CreateInstance<DoCreateUserControl>();
        Texture2D icon = (Texture2D)AssetDatabase.LoadAssetAtPath("Packages/com.noesis.noesisgui/Editor/icon.png", typeof(Texture2D));
        ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, action, "UserControl.xaml", icon, null);
    }
}
