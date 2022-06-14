async function Storey() {

    await jQuery.post({
        url: 'api/Virtuoso/ReasonerStorey',
        contentType: "application/json",
        data: JSON.stringify({ 'graph1': graphURI1, 'graph2': graphURI2, 'Identitykey': Identitykey }),
        success: function(msg) {
            var result = msg["ResultType"];
            document.getElementById('resultresoner').innerHTML = result;
            if (result == "success") {
            }
            console.log(msg);
        }
    });
}
