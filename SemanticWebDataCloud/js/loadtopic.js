function loadtopic() {
    //var myId = this.viewer.getSelection();
    var uId;
    var view_contents;
    var contents_properties;

    setUid();

    async function getViewerId(access_token) {
        await jQuery.get({
            url: 'https://developer.api.autodesk.com/modelderivative/v2/designdata/' + urn + '/manifest',
            headers: {
                'Authorization': 'Bearer ' + access_token
            },
            success: function (res) {
                view_contents = res;
            },
            error: function (err) {
                console.log(err);
            }
        })
    }

    async function getProperty(access_token, guid) {
        await jQuery.get({
            url: 'https://developer.api.autodesk.com/modelderivative/v2/designdata/' + urn + '/metadata/' + guid + '/properties',
            headers: {
                'Authorization': 'Bearer ' + access_token
            },
            success: function (res) {
                console.log(res);
                contents_properties = res;
            },
            error: function (err) {
                console.log(err)
                console.log(err.responseJSON)
            }
        });
    }

    async function setUid() {
        await getForgeToken(function (access_token) {
            getViewerId(access_token).then(() => {
                console.log(view_contents);
                uId = view_contents.derivatives[0].children[2].children[1].guid;
                console.log(uId);
                getUid();

            })
        })
    }

    function getUid() {
        console.log(uId);
        getForgeI_Token(function (access_token) {
            getProperty(access_token, uId).then(() => {
                var getThisproperties = contents_properties.data.collection
                console.log(getThisproperties);
            })
        });
    }
}
function getAllLeafComponents(viewer, callback) {
    var cbCount = 0;
    var tree;
    var jsData = []

    function getLeafComponentsRec(current, parent) {
        cbCount++;
        if (tree.getChildCount(current) != 0) {
            tree.enumNodeChildren(current, function (children) {
                getLeafComponentsRec(children, current);
            }, false);
        }
        var nodeName = viewer.model.getInstanceTree().getNodeName(current)
        jsData.push({ id: current, parent: parent, text: nodeName })

        if (--cbCount == 0) callback(jsData);
    }
    viewer.getObjectTree(function (objectTree) {
        tree = objectTree;
        var rootId = tree.getRootId()
        var nodeName = viewer.model.getInstanceTree().getNodeName(rootId)
        jsData.push({ id: rootId, parent: '#', text: nodeName })
        var allLeafComponents = getLeafComponentsRec(rootId, '#');

    });
}