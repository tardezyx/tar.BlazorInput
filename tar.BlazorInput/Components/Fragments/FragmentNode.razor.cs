using Microsoft.AspNetCore.Components;

namespace tar.BlazorInput.Components.Fragments {
  public partial class FragmentNode {
    #region --- constructor -----------------------------------------------------------------------
    public FragmentNode() {

    }

    public FragmentNode(string id, FragmentNode? parent, string content, bool bold = false) {
      Content = content;
      Id      = id;
      Parent  = parent;
      SetBold = bold;
    }
    #endregion
    #region --- fields ----------------------------------------------------------------------------
    private string _class = string.Empty;
    #pragma warning disable IDE0052 // reference not recognized by IDE
    private ElementReference _reference;
    #pragma warning restore IDE0052
    // private RenderFragment   _renderFragment = builder => { builder.AddContent(0, string.Empty); };
    #endregion
    #region --- properties ------------------------------------------------------------------------
    internal List<FragmentNode> Ancestry    { get; private set; } = [];
    internal bool               IsBold      { get; private set; } = false;
    internal bool               IsCode      { get; private set; } = false;
    internal bool               IsCodeblock { get; private set; } = false;
    internal bool               IsItalic    { get; private set; } = false;
    internal bool               IsLink      { get; private set; } = false;
    internal bool               IsQuote     { get; private set; } = false;

    [Parameter] public List<FragmentNode> Children     { get; set; } = [];
    [Parameter] public string             Content      { get; set; } = string.Empty;
    [Parameter] public string             Id           { get; set; } = string.Empty;
    [Parameter] public bool               SetBold      { get; set; } = false;
    [Parameter] public bool               SetCode      { get; set; } = false;
    [Parameter] public bool               SetCodeblock { get; set; } = false;
    [Parameter] public bool               SetItalic    { get; set; } = false;
    [Parameter] public bool               SetLink      { get; set; } = false;
    [Parameter] public bool               SetQuote     { get; set; } = false;
    [Parameter] public FragmentNode?      Parent       { get; set; }
    #endregion
    
    #region --- add children ----------------------------------------------------------------------
    internal void AddChildren(FragmentNode node) {
      Children.Add(node);
      node.CheckFormats();
      Refresh();
    }
    #endregion
    #region --- check formats ---------------------------------------------------------------------
    internal void CheckFormats() {
      FragmentNode? currentNode = this;

      while (currentNode is not null && !string.IsNullOrEmpty(currentNode.Id)) {
        Ancestry.Add(currentNode);

        if (currentNode.IsBold)      { IsBold      = true; }
        if (currentNode.IsCode)      { IsCode      = true; }
        if (currentNode.IsCodeblock) { IsCodeblock = true; }
        if (currentNode.IsItalic)    { IsItalic    = true; }
        if (currentNode.IsLink)      { IsLink      = true; }
        if (currentNode.IsQuote)     { IsQuote     = true; }

        currentNode = currentNode.Parent;
      }
    }
    #endregion
    #region --- find node -------------------------------------------------------------------------
    internal FragmentNode? FindNode(string id) {
      if (Id == id) {
        return this;
      }

      foreach (FragmentNode children in Children) {
        if (children.FindNode(id) is FragmentNode result) {
          return result;
        }
      }

      return null;
    }
    #endregion
    #region --- get next node ---------------------------------------------------------------------
    internal FragmentNode? GetNextNode() {
      if (Children.Count > 0) {
        return Children[0];
      }

      if (GetNextSibling() is FragmentNode sibling) {
        return sibling;
      }

      if (Parent is not null) {
        return Parent.GetNextNode();
      }

      return null;
    }
    #endregion
    #region --- get next sibling ------------------------------------------------------------------
    internal FragmentNode? GetNextSibling() {
      if (
        Parent is not null
        && Parent.Children.FindIndex(x => x == this) is int index
        && index < Parent.Children.Count - 1
      ) {
        return Parent.Children[index + 1];
      }

      return null;
    }
    #endregion
    #region --- get previous node -----------------------------------------------------------------
    internal FragmentNode? GetPreviousNode() {
      if (GetPreviousSibling() is FragmentNode sibling) {
        return sibling;
      }

      if (Parent is not null) {
        return Parent.GetPreviousNode();
      }

      return null;
    }
    #endregion
    #region --- get previous sibling --------------------------------------------------------------
    internal FragmentNode? GetPreviousSibling() {
      if (
        Parent is not null
        && Parent.Children.FindIndex(x => x == this) is int index
        && index > 0
      ) {
        return Parent.Children[index - 1];
      }

      return null;
    }
    #endregion
    #region --- on initialized --------------------------------------------------------------------
    protected override void OnInitialized() {
      base.OnInitialized();

      if      (SetBold)      { IsBold      = true; _class = "b"; }
      else if (SetCode)      { IsCode      = true; _class = "code"; }
      else if (SetCodeblock) { IsCodeblock = true; _class = "codeblock"; }
      else if (SetItalic)    { IsItalic    = true; _class = "i"; }
      else if (SetLink)      { IsLink      = true; _class = "a"; }
      else if (SetQuote)     { IsQuote     = true; _class = "quote"; }

      // renderFragment = builder => {
      //   builder.OpenElement(1, "div");
      //   builder.AddAttribute(2, "class", _class);
      //   // builder.AddAttribute(3, "onclick", EventCallback.Factory.Create(
      //   //   this,
      //   //   (e) => OnClick(e))
      //   // );
      //   builder.AddAttribute(3, "onfocus", EventCallback.Factory.Create(
      //     this,
      //     (e) => SetHasFocus(e))
      //   );
      //   builder.AddAttribute(4, "onblur", EventCallback.Factory.Create(
      //     this,
      //     (e) => SetHasFocus(e))
      //   );
      //   builder.AddContent(5, Content);
      //   builder.CloseElement();
      // };
    }
    #endregion
    #region --- refresh ---------------------------------------------------------------------------
    internal void Refresh() {
      bool succeeded = false;
      FragmentNode? currentNode = this;

      while (currentNode is not null && !succeeded) {
        try {
          currentNode.StateHasChanged();
          succeeded = true;
        } catch {
          currentNode = currentNode.Parent;
        }
      }
    }
    #endregion
    #region --- update content --------------------------------------------------------------------
    internal void UpdateContent(string content) {
      Content = content;
      Refresh();
    }
    #endregion
  }
}