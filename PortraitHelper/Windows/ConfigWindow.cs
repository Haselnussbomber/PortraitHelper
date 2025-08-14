using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using HaselCommon.Windows;
using Microsoft.Extensions.Hosting;
using PortraitHelper.Config;

namespace PortraitHelper.Windows;

[RegisterSingleton<IHostedService>(Duplicate = DuplicateStrategy.Append), AutoConstruct]
public partial class ConfigWindow : SimpleWindow, IHostedService
{
    private readonly IDalamudPluginInterface _pluginInterface;
    private readonly IServiceProvider _serviceProvider;
    private readonly PluginConfig _pluginConfig;
    private readonly TextService _textService;

    [AutoPostConstruct]
    private void Initialize()
    {
        AllowClickthrough = false;
        AllowPinning = false;

        Flags |= ImGuiWindowFlags.AlwaysAutoResize;

        Size = new Vector2(380, -1);
        SizeCondition = ImGuiCond.Appearing;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _pluginInterface.UiBuilder.OpenConfigUi += Toggle;

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _pluginInterface.UiBuilder.OpenConfigUi -= Toggle;

        return Task.CompletedTask;
    }

    public override void Draw()
    {
        // ShowAlignmentTool
        if (ImGui.Checkbox(_textService.Translate("Config.ShowAlignmentTool"), ref _pluginConfig.ShowAlignmentTool))
        {
            _pluginConfig.Save();
        }

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        var cursorPos = ImGui.GetCursorPos();
        var contentAvail = ImGui.GetContentRegionAvail();

        ImGuiUtils.DrawLink("GitHub", _textService.Translate("ConfigWindow.GitHubLink.Tooltip"), "https://github.com/Haselnussbomber/PortraitHelper");
        ImGui.SameLine();
        ImGui.TextUnformatted("•");
        ImGui.SameLine();
        ImGuiUtils.DrawLink("Ko-fi", _textService.Translate("ConfigWindow.KoFiLink.Tooltip"), "https://ko-fi.com/haselnussbomber");
        ImGui.SameLine();
        ImGui.TextUnformatted("•");
        ImGui.SameLine();
        ImGui.TextUnformatted(_textService.Translate("ConfigWindow.Licenses"));
        if (ImGui.IsItemHovered())
        {
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
            if (ImGui.IsMouseReleased(ImGuiMouseButton.Left) && _serviceProvider.TryGetService<LicensesWindow>(out var licensesWindow))
            {
                Task.Run(licensesWindow.Toggle);
            }
        }

        var version = Assembly.GetExecutingAssembly().GetName().Version;
        if (version != null)
        {
            var versionString = "v" + version.ToString(3);
            ImGui.SetCursorPos(cursorPos + contentAvail - ImGui.CalcTextSize(versionString));
            ImGuiUtils.TextUnformattedDisabled(versionString);
        }
    }
}
