class ElementQuery extends Autodesk.Viewing.Extension {
    constructor(viewer, options) {
        super(viewer, options);
        this._group = null;
        this._button = null;
    }

    load() {
        console.log('HandleSelectionExtension has been loaded');
        return true;
    }



    unload() {
        // Clean our UI elements if we added any
        if (this._group) {
            this._group.removeControl(this._button);
            if (this._group.getNumberOfControls() === 0) {
                this.viewer.toolbar.removeControl(this._group);
            }
        }
        console.log('HandleSelectionExtension has been unloaded');
        return true;
    }

    onToolbarCreated() {
        // Create a new toolbar group if it doesn't exist
        this._group = this.viewer.toolbar.getControl('allMyAwesomeExtensionsToolbar');
        if (!this._group) {
            this._group = new Autodesk.Viewing.UI.ControlGroup('allMyAwesomeExtensionsToolbar');
            this.viewer.toolbar.addControl(this._group);
        }

        // Add a new button to the toolbar group
        this._button = new Autodesk.Viewing.UI.Button('Query');
        this._button.onClick = (ev) => {
            // Get current selection
            const selection = this.viewer.getSelection();
            this.viewer.clearSelection();
            // Anything selected?
            if (selection.length == 1) {
                let isolated = [];
                var ids_query = "";
                // Iterate through the list of selected dbIds
                selection.forEach((dbId) => {
                    // Get properties of each dbId
                    this.viewer.getProperties(dbId, (props) => {
                        // Output properties to console
                        console.log(props);
                        //クエリコードを作成
                        ids_query = ids_query + "construct{?a ?c ?d.}where{?a ?b \"" + props.externalId + "\".?a ?c ?d.}";
                        // Ask if want to isolate
                        if (confirm(`Isolate ${props.name} (${props.externalId})?`)) {
                            isolated.push(dbId);
                            this.viewer.isolate(isolated);
                            document.getElementById('query').innerHTML = ids_query;
                            document.getElementById('query-button').click();
                          
                        }
                    });
                });



            } else {
                // If nothing selected, restore
                this.viewer.isolate(0);
            }
        };
        this._button.setToolTip('ElementQuery');
        this._button.addClass('handleSelectionExtensionIcon');
        this._group.addControl(this._button);
    }
}

Autodesk.Viewing.theExtensionManager.registerExtension('ElementQuery', ElementQuery);
