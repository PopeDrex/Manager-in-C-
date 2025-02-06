namespace PassMan;

public partial class Form1 : Form
{
    public Form1()
{
    InitializeComponent();

    string iconPath = Path.Combine(Application.StartupPath, "icon.ico");
    if (File.Exists(iconPath))
    {
        this.Icon = new Icon(iconPath);
    }
    
} 

}