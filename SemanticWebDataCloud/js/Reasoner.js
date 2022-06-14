var result_json;
var graphURI1;
var graphURI2;
var Identitykey;
var Start_Phase;

function reasoner() {
    graphURI1 = document.getElementById("URIselect1").value;
    graphURI2 = document.getElementById("URIselect2").value;
    Identitykey = document.getElementById('Identitykey').value;
    Start_Phase = document.getElementById('Start_Phase').value;
    ConvertLBD();

    
    
        //こんばーとLBD 
    async function ConvertLBD() {
        await jQuery.post({
            url: 'api/Virtuoso/ConvertLBD',
            contentType: "application/json",
            data: JSON.stringify({ 'graph1': graphURI1, 'graph2': graphURI2, 'Identitykey': Identitykey }),
            success: function (msg) {
                var result = msg["Result"];
                document.getElementById('resultresoner').innerHTML = result;
                
                if (result == "success") {
                    
                    Reasoner();
                }
                
            }
        });
    }
    
    function Reasoner() {

        if (Start_Phase == "Site") {

        }
        else if (Start_Phase == "Building") {
            Building();

        } else if (Start_Phase == "Storey") {
            Storey();

        } else if (Start_Phase == "Space") {
            Space();

        } else if (Start_Phase == "Type") {
            Type();
        }
        else if (Start_Phase == "Element") {
            Element();

        }
        

             
    }
}
 //SiteのReasoner
function Site() {

    jQuery.post({
        url: 'api/Virtuoso/ReasonerSite',
        contentType: "application/json",
        data: JSON.stringify({ 'graph1': graphURI1, 'graph2': graphURI2, 'Start_Phase': Start_Phase, 'Identitykey': Identitykey }),
        success: function (msg) {
            var result = msg["ResultType"];
            document.getElementById('resultresoner').innerHTML = result;
            if (result == "success") {
                Building();
            }
            else if (result == "select") {
                showselectURI(msg, "Site");
                result_json = msg;

            }
            console.log("Site");
            console.log(msg);
        }
    });
}
 //BuildingのReasoner
function Building() {

    jQuery.post({
        url: 'api/Virtuoso/ReasonerBuilding',
        contentType: "application/json",
        data: JSON.stringify({ 'graph1': graphURI1, 'graph2': graphURI2, 'Start_Phase': Start_Phase, 'Identitykey': Identitykey }),
        success: function (msg) {
            var result = msg["ResultType"];
            document.getElementById('resultresoner').innerHTML = result;
            if (result == "success") {
                Storey();
            }
            else if (result == "select") {
                showselectURI(msg, "Building");
                result_json = msg;

            }
            console.log("Building");
            console.log(msg);
        }
    });
}

//StoreyのReasoner
function Storey() {
    
    jQuery.post({
        url: 'api/Virtuoso/ReasonerStorey',
        contentType: "application/json",
        data: JSON.stringify({ 'graph1': graphURI1, 'graph2': graphURI2, 'Start_Phase': Start_Phase, 'Identitykey': Identitykey }),
        success: function (msg) {
            var result = msg["ResultType"];
            document.getElementById('resultresoner').innerHTML = result;
            if (result == "success") {
                Space();
            }
            else if (result == "select") {
                showselectURI(msg, "Storey");
                result_json = msg;
            }
            console.log("Storey");
            console.log(msg);
        }
    });
}

//SpaceのReasoner
function Space() {

    jQuery.post({
        url: 'api/Virtuoso/ReasonerSpace',
        contentType: "application/json",
        data: JSON.stringify({ 'graph1': graphURI1, 'graph2': graphURI2, 'Start_Phase': Start_Phase, 'Identitykey': Identitykey }),
        success: function (msg) {
            var result = msg["ResultType"];
            document.getElementById('resultresoner').innerHTML = result;
            if (result == "success") {
                Type();
            }
            else if (result == "select") {
                showselectURI(msg, "Space");
                result_json = msg;   
            }
            console.log("Space");
            console.log(msg);
        }
    });

    
}

//TypeのReasoner
function Type() {

    jQuery.post({
        url: 'api/Virtuoso/ReasonerType',
        contentType: "application/json",
        data: JSON.stringify({ 'graph1': graphURI1, 'graph2': graphURI2, 'Start_Phase': Start_Phase, 'Identitykey': Identitykey }),
        success: function (msg) {
            var result = msg["ResultType"];
            document.getElementById('resultresoner').innerHTML = result;
            if (result == "success") {
                Element();

            }
            else if (result == "select") {
                showselectURI(msg, "Type");
                result_json = msg;
            }
            console.log("Type");
            console.log(msg);
        }
    });


}

//ElementのReasoner
function Element() {

    jQuery.post({
        url: 'api/Virtuoso/ReasonerElement',
        contentType: "application/json",
        data: JSON.stringify({ 'graph1': graphURI1, 'graph2': graphURI2, 'Start_Phase': Start_Phase, 'Identitykey': Identitykey }),
        success: function (msg) {
            var result = msg["ResultType"];
            document.getElementById('resultresoner').innerHTML = result;
            if (result == "success") {
                document.getElementById('resultresoner').innerHTML = "Fin";

            }
            else if (result == "select") {
                showselectURI(msg, "Element");
                result_json = msg;  
            }
            console.log("Element");
            console.log(msg);
        }
    });

    
}


function nextreasoner() {
    var save_graph = "{";
    var num_json = 1;
    result_json1 = result_json;
    for (URI1 in result_json1["Variables"]) {
        const selection = document.getElementById(URI1).value;
        if (num_json < Object.keys(result_json["Variables"]).length) {
            save_graph += "\"" + URI1 + "\":\"" + selection + "\",";
            num_json++;
        }
        else {
            save_graph += "\"" + URI1 + "\":\"" + selection + "\"";
        }
    }
    const phase_reasoner = document.getElementById("phase").value;
    save_graph += "}";

    graphUpdate(save_graph, phase_reasoner);
    

    $('#fullOverlay').css({
        'display': 'none',
        'z-index': -1
    });
    //console.log(save_graph);
    const oldtable = document.getElementById('selection');
    oldtable.innerHTML = '';
    const obj = JSON.parse(save_graph);
    console.log("save_graph");
    console.log(obj);
    if (phase_reasoner == "Building") {

        Storey();
    }
    else if (phase_reasoner == "Storey") {

        Space();
    }
    else if (phase_reasoner == "Space") {

        Type();
    }
    else if (phase_reasoner == "Type") {
        
        Element();
    }
    else if (phase_reasoner == "Element") {
        document.getElementById('resultresoner').innerHTML = "Fin";
    }
    else if (phase_reasoner == "Site") {
        Building();
    }

}

function showselectURI(result,phase) {
    for (URI1 in result["Variables"]) {
        //divとselectを作成selectにIdを振る
        var div = document.createElement('div');
        var h4 = document.createElement('h4');
        var p = document.createElement('p');
        h4.textContent = phase;
        h4.value = phase;
        h4.id = "phase";
        div.appendChild(h4);
        p.textContent = URI1;
        div.appendChild(p);
        var select = document.createElement('select');
        select.id = URI1;
        for (key in result["Variables"][URI1]) {
            var option = document.createElement('option');
            option.textContent = result["Variables"][URI1][key];
            option.value = result["Variables"][URI1][key];
            select.appendChild(option);

        }
        div.appendChild(select);
        document.getElementById('selection').appendChild(div);
    }
    $('#fullOverlay').css({
        'display': 'block',
        'z-index': 2147483647
    });
}

function changekey() {
    var querycode = ""
    var i = document.getElementById("Identitykey_select").value;
    if (i == "Null") {
        querycode = "";
    }
    else if (i == "URI") {
        querycode = "http://lbd.arch.rwth-aachen.de/props#URI";
    }

    // p要素にvauleを出力
    document.getElementById('Identitykey').value= querycode;
}

async function graphUpdate(graph,phase_reasoner) {

    await jQuery.post({
        url: 'api/Virtuoso/update',
        contentType: "application/json",
        data: JSON.stringify({ 'phase': phase_reasoner, 'graph': graph }),
        success: function (msg) {
            var result = msg["ResultType"];
            
            console.log(result);
        }
    });
}
