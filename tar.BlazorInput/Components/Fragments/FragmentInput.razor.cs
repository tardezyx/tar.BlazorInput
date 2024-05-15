using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using tar.BlazorInput.Models;

namespace tar.BlazorInput.Components.Fragments {
  public partial class FragmentInput {
    #region --- fields ----------------------------------------------------------------------------
    private          ElementReference _inputReference;
    private          FragmentNode     _inputFragmentNode = new("editor", null, string.Empty);
    private readonly DomRange         _range             = new();
    #endregion

    #region --- handle ignorable key --------------------------------------------------------------
    private static bool HandleIgnorableKey(string key) {
      List<string> keys = [
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

      return keys.Contains(key);
    }
    #endregion
    #region --- handle navigation key -------------------------------------------------------------
    private async Task<bool> HandleNavigationKey(KeyboardEventArgs e) {
      List<string> keys = [
        "ArrowDown",
        "ArrowLeft",
        "ArrowRight",
        "ArrowUp",
        "End",
        "Home"
      ];

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

      return keys.Contains(e.Key);
    }
    #endregion
    #region --- handle special key ----------------------------------------------------------------
    private static async Task<bool> HandleSpecialKey(string key) {
      List<string> keys = [
        "Backspace",
        "Delete",
        "Enter",
        "Tab"
      ];

      if (key.Equals("Backspace")) {
      }

      if (key.Equals("Delete")) {
      }

      if (key.Equals("Enter")) {
      }

      if (key.Equals("Tab")) {

      }

      return keys.Contains(key);
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

      if (await HandleSpecialKey(e.Key)) {
        return;
      }

      await _range.InsertText(e.Key);

      StateHasChanged();
    }
    #endregion
  }
}