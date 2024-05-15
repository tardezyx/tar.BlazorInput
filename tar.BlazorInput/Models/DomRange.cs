using Microsoft.JSInterop;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using tar.BlazorInput.Components.Fragments;

namespace tar.BlazorInput.Models {
  public class DomRange : INPC {
    #region --- fields ----------------------------------------------------------------------------
    private FragmentNode?          _editor;
    private string                 _endId       = string.Empty;
    private int                    _endOffset   = 0;
    private string                 _focusId     = string.Empty;
    private int                    _focusOffset = 0;
    private JsonSerializerOptions? _jsonOptions;
    private IJSRuntime?            _jsRuntime;
    private string                 _startId     = string.Empty;
    private int                    _startOffset = 0;
    #endregion
    #region --- properties ------------------------------------------------------------------------
    public string EndId       { get { return _endId;       } set { SetField(ref _endId,       value); } }
    public int    EndOffset   { get { return _endOffset;   } set { SetField(ref _endOffset,   value); } }
    public string FocusId     { get { return _focusId;     } set { SetField(ref _focusId,     value); } }
    public int    FocusOffset { get { return _focusOffset; } set { SetField(ref _focusOffset, value); } }
    public string StartId     { get { return _startId;     } set { SetField(ref _startId,     value); } }
    public int    StartOffset { get { return _startOffset; } set { SetField(ref _startOffset, value); } }

    internal FragmentNode?      EndNode      { get; set; }
    internal FragmentNode?      FocusNode    { get; set; }
    internal List<FragmentNode> NodesBetween { get; set; } = [];
    internal FragmentNode?      StartNode    { get; set; }
    #endregion

    #region --- extract to bold -------------------------------------------------------------------
    internal static void ExtractToBold(FragmentNode node, int start, int end) {
      string content = node.Content[start..end];
      node.UpdateContent(node.Content[..start]);

      FragmentNode newNode = new(
        Guid.NewGuid().ToString(),
        node,
        content,
        true
      );

      node.AddChildren(newNode);
    }
    #endregion
    #region --- format bold -----------------------------------------------------------------------
    internal void FormatBold() {
      if (StartNode is not null && !StartNode.IsBold) {
        ExtractToBold(StartNode, StartOffset, StartNode.Content.Length -1);
      }
    }
    #endregion
    #region --- get nodes -------------------------------------------------------------------------
    internal void GetNodes() {
      if (_editor is null) {
        return;
      }

      EndNode   = _editor.FindNode(EndId);
      FocusNode = _editor.FindNode(FocusId);
      StartNode = _editor.FindNode(StartId);

      GetNodesBetween();
    }
    #endregion
    #region --- get nodes between -----------------------------------------------------------------
    private void GetNodesBetween() {
      if (StartNode is not null && StartNode != EndNode) {
        FragmentNode? currentNode = StartNode;
        while (currentNode is not null && currentNode != _editor) {
          if (EndNode is not null && EndNode.Ancestry.Contains(currentNode)) {
            break;
          }

          if (!StartNode.Ancestry.Contains(currentNode)) {
            NodesBetween.Add(currentNode);
          }
          currentNode = currentNode.GetNextNode();
        }
      }
    }
    #endregion
    #region --- init ------------------------------------------------------------------------------
    internal void Init(FragmentNode editor, IJSRuntime jsRuntime) {
      _editor    = editor;
      _jsRuntime = jsRuntime;
    }
    #endregion
    #region --- insert text ----------------------------------------------------------- (async) ---
    internal async Task InsertText(string text) {
      if (_editor is null) {
        return;
      }

      if (string.IsNullOrEmpty(StartId) || StartId.Equals("inputEditor")) {
        FragmentNode newNode = new(
          Guid.NewGuid().ToString(),
          _editor,
          text
        );
        // FragmentNode newNode = new() {
        //   Id      = Guid.NewGuid().ToString(),
        //   Parent  = _inputFragmentNode,
        //   Content = e.Key
        // };
        _editor.AddChildren(newNode);
        await SetPosition(newNode, EndOffset + text.Length);
      } else if (EndNode is not null) {
        string newContent = EndNode.Content.Insert(EndOffset, text);
        EndNode.UpdateContent(newContent);
        await SetPosition(EndNode, EndOffset + text.Length);
      }
    }
    #endregion
    #region --- move position to left ------------------------------------------------- (async) ---
    internal async Task MovePositionToLeft() {
      if (FocusNode is not null && FocusOffset > 0) {
        await SetPosition(FocusNode, FocusOffset - 1);
      }
    }
    #endregion
    #region --- move position to right ------------------------------------------------ (async) ---
    internal async Task MovePositionToRight() {
      if (FocusNode is not null && FocusOffset < FocusNode.Content.Length) {
        await SetPosition(FocusNode, FocusOffset + 1);
      }
    }
    #endregion
    #region --- remove range ---------------------------------------------------------- (async) ---
    internal async Task RemoveRange() {
      foreach (FragmentNode node in NodesBetween) {
        node.Parent?.Children.Remove(node);
      }

      if (StartNode is not null && StartNode == EndNode && StartOffset != EndOffset) {
        // update text
        string prefix = StartNode.Content[..StartOffset];
        string suffix = StartNode.Content[EndOffset..];
        int    newPos = StartOffset;
        StartNode.UpdateContent(prefix + suffix);

        // set position to original offset
        await SetPosition(StartNode, newPos);
      }

      await Update();
    }
    #endregion
    #region --- set position ---------------------------------------------------------- (async) ---
    internal async Task SetPosition(string elementId, int offset) {
      if (_jsRuntime is null) {
        return;
      }

      const string js = @"export function SetPosition(/*string*/elementId, /*int*/offset) {
        let element = document.getElementById(elementId);
        let child = element.childNodes[0];
        window.getSelection().setPosition(child, offset);
      }";

      IJSObjectReference jsObject = await _jsRuntime.InvokeAsync<IJSObjectReference>(
        "import",
        $"data:application/javascript;base64,{Convert.ToBase64String(Encoding.UTF8.GetBytes(js))}"
      );

      await jsObject.InvokeVoidAsync("SetPosition", [elementId, offset]);
    }
    #endregion
    #region --- set position ---------------------------------------------------------- (async) ---
    private async Task SetPosition(FragmentNode node, int offset) {
      await SetPosition(node.Id, offset);
    }
    #endregion
    #region --- update ---------------------------------------------------------------- (async) ---
    internal async Task Update() {
      if (_jsRuntime is null) {
        return;
      }

      if (_jsonOptions is null) {
        TextEncoderSettings encoderSettings = new();
          encoderSettings.AllowCharacters('\'');
          encoderSettings.AllowRange(UnicodeRanges.All);

        _jsonOptions = new() {
          Converters = {
            new JsonStringEnumConverter()
          },
          Encoder = JavaScriptEncoder.Create(encoderSettings),
          PropertyNameCaseInsensitive = true,
          WriteIndented = true
        };
      }

      const string js = @"export function GetRange() {
        let selection = window.getSelection();
        let range = selection.getRangeAt(0);

        let result = {
          endId: range.endContainer.id
            ? range.endContainer.id
            : range.endContainer.parentNode.id,
          endOffset: range.endOffset,
          focusId: selection.focusNode.id
            ? selection.focusNode.id
            : selection.focusNode.parentNode.id,
          focusOffset: selection.focusOffset,
          startId: range.startContainer.id
            ? range.startContainer.id
            : range.startContainer.parentNode.id,
          startOffset: range.startOffset
        }

        return result;
      }";

      IJSObjectReference jsObject = await _jsRuntime.InvokeAsync<IJSObjectReference>(
        "import",
        $"data:application/javascript;base64,{Convert.ToBase64String(Encoding.UTF8.GetBytes(js))}"
      );

      if (
        await jsObject.InvokeAsync<JsonElement>("GetRange") is JsonElement jsonElement
        && JsonSerializer.Deserialize<DomRange>(jsonElement, _jsonOptions) is DomRange domRange
      ) {
        EndId       = domRange.EndId;
        EndOffset   = domRange.EndOffset;
        FocusId     = domRange.FocusId;
        FocusOffset = domRange.FocusOffset;
        StartId     = domRange.StartId;
        StartOffset = domRange.StartOffset;

        GetNodes();
      }
    }
    #endregion
  }
}