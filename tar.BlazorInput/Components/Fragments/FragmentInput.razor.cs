using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using tar.BlazorInput.Models;

namespace tar.BlazorInput.Components.Fragments {
  public partial class FragmentInput {
    #region --- fields ----------------------------------------------------------------------------
    private          FragmentNode     _inputFragmentNode = new("editor", null, string.Empty);
    #pragma warning disable IDE0052 // reference not recognized by IDE
    private          ElementReference _inputReference;
    #pragma warning restore IDE0052
    private          List<string>     _keysIgnorable     = [];
    private          List<string>     _keysNavigation    = [];
    private          List<string>     _keysSpecial       = [];
    private readonly DomRange         _range             = new();
    #endregion

    #region --- handle ignorable key --------------------------------------------------------------
    private bool HandleIgnorableKey(string key) {
      if (_keysIgnorable.Count == 0) {
        _keysIgnorable = [
          "Alt",
          "AltGraph",
          "Clear",
          "ContextMenu",
          "Control",
          "Dead",
          "Escape",
          "F1",
          "F2",
          "F3",
          "F4",
          "F5",
          "F6",
          "F7",
          "F8",
          "F9",
          "F10",
          "F11",
          "F12",
          "Insert",
          "NumLock",
          "OS",
          "PageDown",
          "PageUp",
          "Pause",
          "ScrollLock",
          "Shift",
        ];
      }

      return _keysIgnorable.Contains(key);
    }
    #endregion
    #region --- handle navigation key -------------------------------------------------------------
    private async Task<bool> HandleNavigationKey(KeyboardEventArgs e) {
      if (_keysNavigation.Count == 0) {
        _keysNavigation = [
          "ArrowDown",
          "ArrowLeft",
          "ArrowRight",
          "ArrowUp",
          "End",
          "Home"
        ];
      }

      if (e.Key.Equals("ArrowDown")) {
      }

      if (e.Key.Equals("ArrowLeft")) {
        if (e.ShiftKey) {
        
        } else {
          await _range.MovePositionToLeft();
        }
      }

      if (e.Key.Equals("ArrowRight")) {
        if (e.ShiftKey) {
        
        } else {
          await _range.MovePositionToRight();
        }
      }

      if (e.Key.Equals("ArrowUp")) {
      }

      if (e.Key.Equals("End")) {
      }

      if (e.Key.Equals("Home")) {
      }

      return _keysNavigation.Contains(e.Key);
    }
    #endregion
    #region --- handle special key ----------------------------------------------------------------
    private bool HandleSpecialKey(string key) {
      if (_keysSpecial.Count == 0) {
        _keysSpecial = [
          "Backspace",
          "Delete",
          "Enter",
          "Tab"
        ];
      }

      if (key.Equals("Backspace")) {
      }

      if (key.Equals("Delete")) {
      }

      if (key.Equals("Enter")) {
      }

      if (key.Equals("Tab")) {

      }

      return _keysSpecial.Contains(key);
    }
    #endregion
    #region --- on after render -------------------------------------------------------------------
    protected override void OnAfterRender(bool firstRender) {
      base.OnAfterRender(firstRender);

      if (firstRender) {
        _range.Init(_inputFragmentNode, JSRuntime);
      }
    }
    #endregion
    #region --- on key down -----------------------------------------------------------------------
    internal async Task OnKeyDown(KeyboardEventArgs e) {
      await _range.Update();

      if (await HandleNavigationKey(e) || HandleIgnorableKey(e.Key)) {
        return;
      }

      await _range.RemoveRange();

      if (HandleSpecialKey(e.Key)) {
        return;
      }

      await _range.InsertText(e.Key);

      //StateHasChanged();
    }
    #endregion
  }
}