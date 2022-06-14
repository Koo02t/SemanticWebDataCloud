//クエリ
async function showMessage() {
    const textbox = document.getElementById("query");
    const oldtable = document.getElementById('maintable');
    oldtable.innerHTML = '';
    const inputValue = textbox.value;
    var query = inputValue;
    


    await jQuery.post({
        url: 'api/Virtuoso/jobs',
        contentType: "application/json",
        data: JSON.stringify({ 'Querycode': query }),
        success: function (msg) {
            if (msg["ResultType"] == "Select") {
                var table = document.createElement('table');
                table.className = 'table';
                table.className = 'table-width';

                // ヘッダーを作成
                var tr1 = document.createElement('tr');
                for (key in msg["Variables"]) {
                    // th要素を生成
                    var th = document.createElement('th');
                    // th要素内にテキストを追加
                    th.textContent = msg["Variables"][key];
                    // th要素をtr要素の子要素に追加
                    tr1.appendChild(th);

                }
                table.appendChild(tr1);

                // テーブル本体を作成
                for (key in msg) {
                    // tr要素を生成
                    var tr2 = document.createElement('tr');
                    // th・td部分のループ
                    for (value_key in msg[key]) {
                        if (!(key == "ResultType" || key == "Variables")) {
                            // td要素を生成
                            var td = document.createElement('td');
                            // td要素内にテキストを追加
                            td.textContent = msg[key][value_key];
                            // td要素をtr要素の子要素に追加
                            tr2.appendChild(td);
                        }

                    }


                    // tr要素をtable要素の子要素に追加
                    table.appendChild(tr2);
                }
                document.getElementById('maintable').appendChild(table);
            }
            else if (msg["ResultType"] == "Construct") {
                var table = document.createElement('table');
                table.className = 'table';
                table.className = 'table-width';
                // ヘッダーを作成
                var tr1 = document.createElement('tr');
                var list = ["Subject", "Predicate", "Object"];
                for (let i = 0; i < list.length; i++) {
                    // th要素を生成
                    var th = document.createElement('th');
                    // th要素内にテキストを追加
                    th.textContent = list[i];
                    // th要素をtr要素の子要素に追加
                    tr1.appendChild(th);

                }
                table.appendChild(tr1);

                for (key in msg) {
                    var i = 0;
                    if (!(key == "ResultType")) {
                        for (key_prop in msg[key]) {
                            //一行目(sub,pre,obj)
                            if (i == 0) {
                                if (Object.keys(msg[key][key_prop]).length > 1) {
                                    var j = 0;
                                    for (key_value in msg[key][key_prop]) {
                                        if (j == 0) {
                                            var tr2 = document.createElement('tr');
                                            var td_sub = document.createElement('td');
                                            var td_pre = document.createElement('td');
                                            var td_obj = document.createElement('td');

                                            td_sub.textContent = key;
                                            td_pre.textContent = key_prop;
                                            td_obj.textContent = msg[key][key_prop][key_value];
                                            tr2.appendChild(td_sub);
                                            tr2.appendChild(td_pre);
                                            tr2.appendChild(td_obj);
                                            table.appendChild(tr2);
                                            j++;
                                        }
                                        else {
                                            var tr2 = document.createElement('tr');
                                            var td_sub = document.createElement('td');
                                            var td_pre = document.createElement('td');
                                            var td_obj = document.createElement('td');

                                            td_sub.textContent = "---";
                                            td_pre.textContent = "---";
                                            td_obj.textContent = msg[key][key_prop][key_value];
                                            tr2.appendChild(td_sub);
                                            tr2.appendChild(td_pre);
                                            tr2.appendChild(td_obj);
                                            table.appendChild(tr2);
                                            
                                        }
                                    }

                                }
                                else {
                                    var tr2 = document.createElement('tr');
                                    var td_sub = document.createElement('td');
                                    var td_pre = document.createElement('td');
                                    var td_obj = document.createElement('td');

                                    td_sub.textContent = key;
                                    td_pre.textContent = key_prop;
                                    td_obj.textContent = msg[key][key_prop];
                                    tr2.appendChild(td_sub);
                                    tr2.appendChild(td_pre);
                                    tr2.appendChild(td_obj);
                                    table.appendChild(tr2);
                                }
                                i++
                            }
                            //二行目以降(pre,obj)
                            else if (i > 0) {
                                if (Object.keys(msg[key][key_prop]).length > 1) {
                                    var j = 0;
                                    for (key_value in msg[key][key_prop]) {
                                        if (j == 0) {
                                            var tr2 = document.createElement('tr');
                                            var td_sub = document.createElement('td');
                                            var td_pre = document.createElement('td');
                                            var td_obj = document.createElement('td');

                                            td_sub.textContent = "---";
                                            td_pre.textContent = key_prop;
                                            td_obj.textContent = msg[key][key_prop][key_value];
                                            tr2.appendChild(td_sub);
                                            tr2.appendChild(td_pre);
                                            tr2.appendChild(td_obj);
                                            table.appendChild(tr2);
                                            j++;
                                        }
                                        else {
                                            var tr2 = document.createElement('tr');
                                            var td_sub = document.createElement('td');
                                            var td_pre = document.createElement('td');
                                            var td_obj = document.createElement('td');

                                            td_sub.textContent = "---";
                                            td_pre.textContent = "---";
                                            td_obj.textContent = msg[key][key_prop][key_value];
                                            tr2.appendChild(td_sub);
                                            tr2.appendChild(td_pre);
                                            tr2.appendChild(td_obj);
                                            table.appendChild(tr2);

                                        }
                                    }

                                }
                                else {
                                    var tr2 = document.createElement('tr');
                                    var td_sub = document.createElement('td');
                                    var td_pre = document.createElement('td');
                                    var td_obj = document.createElement('td');

                                    td_sub.textContent = "---";
                                    td_pre.textContent = key_prop;
                                    td_obj.textContent = msg[key][key_prop];
                                    tr2.appendChild(td_sub);
                                    tr2.appendChild(td_pre);
                                    tr2.appendChild(td_obj);
                                    table.appendChild(tr2);
                                }
                                

                            }

                        }
                    }
                 }
               
                 document.getElementById('maintable').appendChild(table);
                
            }

            else {
                 var error = msg["ResultType"];
                 document.getElementById('maintable').innerHTML = error;
            }

        },
        error: function (e) {
            document.getElementById('maintable').innerHTML = "error";
        }
    });
    

}
//クエリコードのサンプル
function change() {
    var querycode = ""
    var i = document.getElementById("querycodesample1").value;
    if (i == "Null") {
        querycode = "";
    }
    else if (i == "Storey") {
        querycode = "select*where{?a a <https://w3id.org/bot#Storey>.}";
    }
    else if (i == "Floorの中のElement") {
        querycode = "CONSTRUCT{?a <https://w3id.org/bot#containsElement> ?c.}{?a a <https://w3id.org/bot#Storey>. ?a <https://w3id.org/bot#containsElement> ?c. }";
    }
  


    // p要素にvauleを出力
    document.getElementById('query').innerHTML = querycode;
}


//ttlデータのアップロード
function upload() {
    var element = document.getElementById("rdffile");

    // 読み込むファイル
    var file = element.files[0]; // 1つ目のファイル

    // FileReaderを作成
    var fileReader = new FileReader();

    // 読み込み完了時のイベント
    fileReader.onload = function() {
        jQuery.post({
            url: 'api/Virtuoso/upload',
            contentType: "application/json",
            data: JSON.stringify({ 'File': this.result.toString() }),
            success: function(msg) {
                
                document.getElementById('result').innerHTML = msg;
                reload();
            }
        });

    };
    // 読み込みを実行
    fileReader.readAsText(file);


}

//グラフツリーのロード
window.onload = function () {
    load();
}
async function load() {
    var a = "";
    await jQuery.post({
        url: 'api/Virtuoso/tree',
        contentType: "application/json",
        data: JSON.stringify({ 'tree': a }),
        success: function (msg) {


            if (!(msg["Result"] == "error")) {


                var ul1 = document.createElement('ul');
                var li1 = document.createElement('li');
                var a1 = document.createElement('a');

                a1.textContent = "db";
                li1.appendChild(a1);
                ul1.appendChild(li1);
                var ul2 = document.createElement('ul');
                var li3 = document.createElement('li');
                var a3 = document.createElement('a');
                a3.textContent = "Ontology";
                li3.appendChild(a3);
                ul1.appendChild(li3);
                var ul3 = document.createElement('ul');




                for (key in msg["Result"]) {
                    var mark = msg["Result"][key];
                    if (msg[mark] == "Data" && msg["Result"][key] != "http://www.openlinksw.com/schemas/virtrdf#" && msg["Result"][key] != "http://www.w3.org/ns/ldp#" && msg["Result"][key] != "http://localhost:8890/sparql" && msg["Result"][key] != "http://localhost:8890/DAV/" && msg["Result"][key] != "http://www.w3.org/2002/07/owl#")
                    {
                        // th要素を生成
                        var li2 = document.createElement('li');
                        var a2 = document.createElement('a');
                        a2.href = msg["Result"][key];
                        // th要素内にテキストを追加
                        a2.textContent = msg["Result"][key];
                        // th要素をtr要素の子要素に追加
                        li2.appendChild(a2);
                        ul2.appendChild(li2);

                        //optionにURIを追加
                        var option1 = document.createElement('option');
                        option1.textContent = msg["Result"][key];
                        option1.value = msg["Result"][key];
                        var option2 = document.createElement('option');
                        option2.textContent = msg["Result"][key];
                        option2.value = msg["Result"][key];
                        document.getElementById('URIselect1').appendChild(option1);
                        document.getElementById('URIselect2').appendChild(option2);
                    }
                    else if (msg["Result"][key] == "http://www.openlinksw.com/schemas/virtrdf#" || msg["Result"][key] == "http://www.w3.org/ns/ldp#" || msg["Result"][key] == "http://localhost:8890/sparql" || msg["Result"][key] == "http://localhost:8890/DAV/" || msg["Result"][key] == "http://www.w3.org/2002/07/owl#"){

                    }
                    else {
                        var li2 = document.createElement('li');
                        var a2 = document.createElement('a');
                        a2.href = msg["Result"][key];
                        // th要素内にテキストを追加
                        a2.textContent = msg["Result"][key];
                        // th要素をtr要素の子要素に追加
                        li2.appendChild(a2);
                        ul3.appendChild(li2);
                        
                    }
                    
                }
                li1.appendChild(ul2);
                li3.appendChild(ul3);
                document.getElementById('graph_tree').appendChild(ul1);
            }
            else {
                document.getElementById('graph_tree').innerHTML = "error";
            }
            
        }

    });
}

//グラフツリーのリロード
function reload() {
    const oldtable1 = document.getElementById('graph_tree');
    const oldtable2 = document.getElementById('URIselect1');
    const oldtable3 = document.getElementById('URIselect2');
    oldtable1.innerHTML = '';
    oldtable2.innerHTML = '';
    oldtable3.innerHTML = '';
    load();
}
//ttlデータのデリート
$(function () {
    
    $('body').on('contextmenu', '#graph_tree ul li ul li a', function (e) {
        e.preventDefault();
        var text = $(this).text();
        var result = window.confirm(text + "を削除しますか？");
        if (result) {

            jQuery.post({
                url: 'api/Virtuoso/Delete',
                contentType: "application/json",
                data: JSON.stringify({ 'graphname': text}),
                success: function (msg) {
                    var result = msg["Result"];
                    document.getElementById('result').innerHTML = result;
                    reload();
                }
            });

        }
        else {
        }
        
    });
});


