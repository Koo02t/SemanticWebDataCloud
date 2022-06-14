using System.Web;
using VDS.RDF;
using VDS.RDF.Query;
using VDS.RDF.Storage;
using System.Web.Http;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing;
using VDS.RDF.Writing.Formatting;

namespace SemanticWebDataCloud.Controllers
{
    public class ReasonerController : ApiController
    {


        // GET: Reasoner
        //データをLBDに変換
        [HttpPost]
        [Route("api/Virtuoso/ConvertLBD")]
        public JObject ConvertLBD([FromBody] graphURI graph)
        {


            JObject Result;
            try
            {
                //string test = "";
                int test_num1 = 0;
                //int test_num2 = 0;
                int basegraph_num = 0;
                //グラフのリスト作成
                List<string> graph_list = new List<string>();
                List<string> base_graph = new List<string>();
                graph_list.Add(graph.graph1);
                graph_list.Add(graph.graph2);
                base_graph.Add(graph.URIDic["inst1"]);
                base_graph.Add(graph.URIDic["inst2"]);

                foreach (string convert_graph in graph_list)
                {


                    Graph g = new Graph { BaseUri = new Uri(base_graph[basegraph_num]) };

                    string querystring = "";
                    VirtuosoManager virtuoso = new VirtuosoManager("localhost", 1111, "DB", "dba", "0443Kou10");

                    //prefixを作成
                    foreach (string i in graph.URIDic.Keys)
                    {
                        querystring += "prefix " + i + ":<" + graph.URIDic[i] + ">";
                    }

                    //クラスのクエリと代替
                    string query = querystring + "CONSTRUCT{?a rdf:type ?d.}{graph<" + convert_graph + ">{?a rdf:type ?d.}}";
                    object queryresult = virtuoso.Query(query);
                    IGraph g_class_query = (IGraph)queryresult;
                    foreach (Triple t in g_class_query.Triples)
                    {
                        test_num1 += 1;
                        INode objnode = t.Object;
                        if (objnode.ToString().Contains(graph.URIDic["uniclass"]) == false && objnode.ToString().Contains(graph.URIDic["bot"]) == false && objnode.ToString().Contains(graph.URIDic["type"]) == false)
                        {

                            string relation_query_string = querystring + "CONSTRUCT{<" + objnode.ToString() + "> ?c ?d.}{<" + objnode.ToString() + "> ?c ?d.}";
                            object relation_query_result = virtuoso.Query(relation_query_string);
                            IGraph g_relation_query = (IGraph)relation_query_result;
                            int num = 0;
                            foreach (Triple i in g_relation_query.Triples)
                            {
                                if (i.Predicate.ToString() == "http://www.w3.org/2002/07/owl#equivalentClass")
                                {
                                    if (i.Object.ToString().Contains(graph.URIDic["bot"]) == true || i.Object.ToString().Contains(graph.URIDic["type"]) == true)
                                    {
                                        IUriNode subject_temp = g.CreateUriNode(UriFactory.Create(t.Subject.ToString()));
                                        IUriNode predicate_temp = g.CreateUriNode(UriFactory.Create(t.Predicate.ToString()));
                                        IUriNode object_temp = g.CreateUriNode(UriFactory.Create(i.Object.ToString()));
                                        g.Assert(new Triple(subject_temp, predicate_temp, object_temp));
                                        num++;
                                    }
                                }
                                else if (i.Predicate.ToString() == "http://www.w3.org/2000/01/rdf-schema#http://www.w3.org/2002/07/owl#subClassOf")
                                {
                                    if (i.Object.ToString().Contains(graph.URIDic["bot"]) == true || i.Object.ToString().Contains(graph.URIDic["type"]) == true)
                                    {
                                        IUriNode subject_temp = g.CreateUriNode(UriFactory.Create(t.Subject.ToString()));
                                        IUriNode predicate_temp = g.CreateUriNode(UriFactory.Create(t.Predicate.ToString()));
                                        IUriNode object_temp = g.CreateUriNode(UriFactory.Create(i.Object.ToString()));
                                        g.Assert(new Triple(subject_temp, predicate_temp, object_temp));
                                        num++;
                                    }
                                    else
                                    {
                                        string subclass_query_string = querystring + "CONSTRUCT{<" + i.Object.ToString() + "> ?c ?d.}{<" + i.Object.ToString() + "> ?c ?d.}";
                                        object subclass_query_result = virtuoso.Query(subclass_query_string);
                                        IGraph subclass_relation_query = (IGraph)subclass_query_result;
                                        foreach (Triple z in subclass_relation_query.Triples)
                                        {
                                            if (z.Predicate.ToString() == "http://www.w3.org/2000/01/rdf-schema#http://www.w3.org/2002/07/owl#subClassOf" || z.Predicate.ToString() == "http://www.w3.org/2002/07/owl#equivalentClass")
                                            {
                                                if (z.Object.ToString().Contains(graph.URIDic["bot"]) == true || z.Object.ToString().Contains(graph.URIDic["type"]) == true)
                                                {
                                                    IUriNode subject_temp = g.CreateUriNode(UriFactory.Create(t.Subject.ToString()));
                                                    IUriNode predicate_temp = g.CreateUriNode(UriFactory.Create(t.Predicate.ToString()));
                                                    IUriNode object_temp = g.CreateUriNode(UriFactory.Create(z.Object.ToString()));
                                                    g.Assert(new Triple(subject_temp, predicate_temp, object_temp));
                                                    num++;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            if (num == 0)
                            {
                                g.Assert(t);
                            }

                        }
                        else
                        {
                            g.Assert(t);
                        }
                    }

                    //オブジェクトプロパティーのクエリと代替
                    query = querystring + "CONSTRUCT{?a ?c ?d.}{graph<" + convert_graph + ">{?a ?c ?d.}{?c rdf:type owl:ObjectProperty.}}";
                    queryresult = virtuoso.Query(query);
                    IGraph g_property_query = (IGraph)queryresult;
                    foreach (Triple t in g_property_query.Triples)
                    {
                        test_num1 += 1;
                        INode prenode = t.Predicate;
                        if (prenode.ToString().Contains(graph.URIDic["uniclass"]) == false && prenode.ToString().Contains(graph.URIDic["bot"]) == false && prenode.ToString().Contains(graph.URIDic["type"]) == false)
                        {
                            int num = 0;
                            string relation_query_string = querystring + "CONSTRUCT{<" + prenode.ToString() + "> ?c ?d.}{<" + prenode.ToString() + "> ?c ?d.}";
                            object relation_query_result = virtuoso.Query(relation_query_string);
                            IGraph g_relation_query = (IGraph)relation_query_result;
                            foreach (Triple i in g_relation_query.Triples)
                            {

                                if (i.Predicate.ToString() == "http://www.w3.org/2002/07/owl#equivalentProperty")
                                {
                                    if (i.Object.ToString().Contains(graph.URIDic["bot"]))
                                    {
                                        IUriNode subject_temp = g.CreateUriNode(UriFactory.Create(t.Subject.ToString()));
                                        IUriNode object_temp = g.CreateUriNode(UriFactory.Create(t.Object.ToString()));
                                        IUriNode predicate_temp = g.CreateUriNode(UriFactory.Create(i.Object.ToString()));
                                        g.Assert(new Triple(subject_temp, predicate_temp, object_temp));
                                        num++;
                                    }
                                }
                                else if (i.Predicate.ToString() == "http://www.w3.org/2000/01/rdf-schema#subPropertyOf")
                                {
                                    if (i.Object.ToString().Contains(graph.URIDic["bot"]) == true || i.Object.ToString().Contains(graph.URIDic["type"]) == true)
                                    {
                                        IUriNode subject_temp = g.CreateUriNode(UriFactory.Create(t.Subject.ToString()));
                                        IUriNode object_temp = g.CreateUriNode(UriFactory.Create(t.Object.ToString()));
                                        IUriNode predicate_temp = g.CreateUriNode(UriFactory.Create(i.Object.ToString()));
                                        g.Assert(new Triple(subject_temp, predicate_temp, object_temp));
                                        num++;
                                    }
                                    else
                                    {
                                        string subclass_query_string = querystring + "CONSTRUCT{<" + i.Object.ToString() + "> ?c ?d.}{<" + i.Object.ToString() + "> ?c ?d.}";
                                        object subclass_query_result = virtuoso.Query(subclass_query_string);
                                        IGraph subclass_relation_query = (IGraph)subclass_query_result;
                                        foreach (Triple z in subclass_relation_query.Triples)
                                        {
                                            if (z.Predicate.ToString() == "http://www.w3.org/2000/01/rdf-schema#subPropertyOf" || z.Predicate.ToString() == "http://www.w3.org/2002/07/owl#equivalentProperty")
                                            {
                                                if (z.Object.ToString().Contains(graph.URIDic["bot"]) == true || z.Object.ToString().Contains(graph.URIDic["type"]) == true)
                                                {
                                                    IUriNode subject_temp = g.CreateUriNode(UriFactory.Create(t.Subject.ToString()));
                                                    IUriNode object_temp = g.CreateUriNode(UriFactory.Create(t.Object.ToString()));
                                                    IUriNode predicate_temp = g.CreateUriNode(UriFactory.Create(z.Object.ToString()));
                                                    g.Assert(new Triple(subject_temp, predicate_temp, object_temp));
                                                    num++;
                                                }
                                            }
                                            else if (z.Predicate.ToString() == "http://www.w3.org/2002/07/owl#inverseOf")
                                            {
                                                IUriNode subject_temp = g.CreateUriNode(UriFactory.Create(t.Subject.ToString()));
                                                IUriNode object_temp = g.CreateUriNode(UriFactory.Create(t.Object.ToString()));
                                                IUriNode predicate_temp = g.CreateUriNode(UriFactory.Create(z.Object.ToString()));
                                                g.Assert(new Triple(object_temp, predicate_temp, subject_temp));
                                                num++;
                                            }
                                        }
                                    }
                                }
                                else if (i.Predicate.ToString() == "http://www.w3.org/2002/07/owl#inverseOf")
                                {
                                    if (i.Object.ToString().Contains(graph.URIDic["bot"]) == true || i.Object.ToString().Contains(graph.URIDic["type"]) == true)
                                    {
                                        IUriNode subject_temp = g.CreateUriNode(UriFactory.Create(t.Subject.ToString()));
                                        IUriNode object_temp = g.CreateUriNode(UriFactory.Create(t.Object.ToString()));
                                        IUriNode predicate_temp = g.CreateUriNode(UriFactory.Create(i.Object.ToString()));
                                        g.Assert(new Triple(object_temp, predicate_temp, subject_temp));
                                        num++;
                                    }
                                    else
                                    {
                                        string subclass_query_string = querystring + "CONSTRUCT{<" + i.Object.ToString() + "> ?c ?d.}{<" + i.Object.ToString() + "> ?c ?d.}";
                                        object subclass_query_result = virtuoso.Query(subclass_query_string);
                                        IGraph subclass_relation_query = (IGraph)subclass_query_result;
                                        foreach (Triple z in subclass_relation_query.Triples)
                                        {
                                            if (z.Predicate.ToString() == "http://www.w3.org/2000/01/rdf-schema#subPropertyOf" || z.Predicate.ToString() == "http://www.w3.org/2002/07/owl#equivalentProperty")
                                            {
                                                if (z.Object.ToString().Contains(graph.URIDic["bot"]) == true || z.Object.ToString().Contains(graph.URIDic["type"]) == true)
                                                {
                                                    IUriNode subject_temp = g.CreateUriNode(UriFactory.Create(t.Subject.ToString()));
                                                    IUriNode object_temp = g.CreateUriNode(UriFactory.Create(t.Object.ToString()));
                                                    IUriNode predicate_temp = g.CreateUriNode(UriFactory.Create(z.Object.ToString()));
                                                    g.Assert(new Triple(object_temp, predicate_temp, subject_temp));

                                                    num++;
                                                }
                                            }
                                            else if (z.Predicate.ToString() == "http://www.w3.org/2002/07/owl#inverseOf")
                                            {
                                                if (z.Object.ToString().Contains(graph.URIDic["bot"]) == true || z.Object.ToString().Contains(graph.URIDic["type"]) == true)
                                                {
                                                    IUriNode subject_temp = g.CreateUriNode(UriFactory.Create(t.Subject.ToString()));
                                                    IUriNode object_temp = g.CreateUriNode(UriFactory.Create(t.Object.ToString()));
                                                    IUriNode predicate_temp = g.CreateUriNode(UriFactory.Create(z.Object.ToString()));
                                                    g.Assert(new Triple(subject_temp, predicate_temp, object_temp));
                                                    num++;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            if (num == 0)
                            {
                                g.Assert(t);
                            }

                        }
                        else
                        {
                            g.Assert(t);
                        }


                    }

                    virtuoso.SaveGraph(g);
                    basegraph_num++;
                }
                //確認
                /*foreach (Triple t in g.Triples)
                {
                    test += "\r\n" + t.ToString();
                    test_num2 += 1;
                }
                */
                string r = "{\"Result\" : \"success\"}";
                Result = JObject.Parse(r.ToString());
            }
            catch (Exception)
            {
                string nullerror = "{\"Result\" : \"error\"}";
                Result = JObject.Parse(nullerror.ToString());

            }

            return Result;
        }

        // GET: Reasoner
        //Siteの検索
        [HttpPost]
        [Route("api/Virtuoso/ReasonerSite")]
        public JObject Site([FromBody] graphURI graph)
        {


            JObject Result;
            try
            {
                //準備
                StringBuilder Resultstring = new StringBuilder();
                Resultstring.AppendLine("{");


                Graph g = new Graph { BaseUri = new Uri(graph.URIDic["sameas"]) };

                string querystring = "";
                VirtuosoManager virtuoso = new VirtuosoManager("localhost", 1111, "DB", "dba", "0443Kou10");

                //prefixを作成
                foreach (string i in graph.URIDic.Keys)
                {
                    querystring += "prefix " + i + ":<" + graph.URIDic[i] + ">";
                }

                //  Buildingのクエリと代替
                string query = querystring + "CONSTRUCT{?a rdf:type bot:Site.}{graph <" + graph.URIDic["inst1"] + "> {?a rdf:type bot:Site.}}";
                object queryresult = virtuoso.Query(query);
                IGraph g_class_query = (IGraph)queryresult;
                //各グラフにあるビル
                Resultstring.AppendLine("\"ResultType\":");

                //識別最下層があるとき
                if (graph.Identitykey != "")
                {
                    Resultstring.Append("\"success\",");
                    Resultstring.AppendLine("\"Variables\":{");
                    int temp_num1 = 1;
                    foreach (Triple t in g_class_query.Triples)
                    {
                        Resultstring.AppendLine("\"" + t.Subject.ToString() + "\":[");
                        INode Site_URI = t.Subject;
                        string query_buil = querystring + "CONSTRUCT{?a rdf:type bot:Site.}{graph<" + graph.graph1 + ">{<" + Site_URI + "> <" + graph.Identitykey + "> ?c.}graph<" + graph.graph2 + ">{?a <" + graph.Identitykey + "> ?c.}graph<" + graph.URIDic["inst2"] + ">{?a rdf:type bot:Site.} }";
                        object queryresult_buil = virtuoso.Query(query_buil);
                        IGraph g_class_query_buil = (IGraph)queryresult_buil;
                        if (g_class_query_buil.IsEmpty == false)
                        {
                            int temp_num = 1;
                            foreach (Triple sameas in g_class_query_buil.Triples)
                            {
                                IUriNode subject_temp = g.CreateUriNode(UriFactory.Create(t.Subject.ToString()));
                                IUriNode predicate_temp = g.CreateUriNode(UriFactory.Create(graph.URIDic["owl"] + "sameAs"));
                                IUriNode object_temp = g.CreateUriNode(UriFactory.Create(sameas.Subject.ToString()));
                                g.Assert(subject_temp, predicate_temp, object_temp);
                                if (temp_num1 < g_class_query.Triples.Count())
                                {
                                    if (temp_num < g_class_query_buil.Triples.Count())
                                    {
                                        Resultstring.AppendLine("\"" + sameas.Subject.ToString() + "\",");
                                        temp_num++;
                                    }
                                    else
                                    {
                                        Resultstring.AppendLine("\"" + sameas.Subject.ToString() + "\"],");
                                        temp_num1++;
                                    }
                                }
                                else
                                {
                                    if (temp_num < g_class_query_buil.Triples.Count())
                                    {
                                        Resultstring.AppendLine("\"" + sameas.Subject.ToString() + "\",");
                                        temp_num++;
                                    }
                                    else
                                    {
                                        Resultstring.AppendLine("\"" + sameas.Subject.ToString() + "\"]");
                                        temp_num1++;
                                    }
                                }

                            }

                        }
                        else
                        {
                            if (temp_num1 < g_class_query.Triples.Count())
                            {
                                Resultstring.AppendLine("],");
                                temp_num1++;
                            }
                            else
                            {
                                Resultstring.AppendLine("]");
                                temp_num1++;
                            }

                        }

                    }
                    Resultstring.AppendLine("}");
                    virtuoso.SaveGraph(g);
                }

                //識別最下層がないとき
                else
                {
                    Resultstring.Append("\"select\",");
                    Resultstring.AppendLine("\"Variables\":{");
                    int temp_num1 = 1;
                    foreach (Triple t in g_class_query.Triples)
                    {
                        Resultstring.AppendLine("\"" + t.Subject.ToString() + "\":[");
                        INode Building_URI = t.Subject;
                        string query_buil = querystring + "construct{?a rdf:type bot:Building.}{graph<" + graph.URIDic["inst2"] + ">{?a rdf:type bot:Building.}}";
                        object queryresult_buil = virtuoso.Query(query_buil);
                        IGraph g_class_query_buil = (IGraph)queryresult_buil;
                        if (g_class_query_buil.IsEmpty == false)
                        {
                            int temp_num = 1;
                            foreach (Triple sameas in g_class_query_buil.Triples)
                            {

                                if (temp_num1 < g_class_query.Triples.Count())
                                {
                                    if (temp_num < g_class_query_buil.Triples.Count())
                                    {
                                        Resultstring.AppendLine("\"" + sameas.Subject.ToString() + "\",");
                                        temp_num++;
                                    }
                                    else
                                    {
                                        Resultstring.AppendLine("\"" + sameas.Subject.ToString() + "\"],");
                                        temp_num1++;
                                    }
                                }
                                else
                                {
                                    if (temp_num < g_class_query_buil.Triples.Count())
                                    {
                                        Resultstring.AppendLine("\"" + sameas.Subject.ToString() + "\",");
                                        temp_num++;
                                    }
                                    else
                                    {
                                        Resultstring.AppendLine("\"" + sameas.Subject.ToString() + "\"]");
                                        temp_num1++;
                                    }
                                }
                            }

                        }
                        else
                        {
                            if (temp_num1 < g_class_query.Triples.Count())
                            {
                                Resultstring.AppendLine("],");
                                temp_num1++;
                            }
                            else
                            {
                                Resultstring.AppendLine("]");
                                temp_num1++;
                            }
                        }

                    }
                    Resultstring.AppendLine("}");
                }
                Resultstring.AppendLine("}");
                Result = JObject.Parse(Resultstring.ToString());


                //string r = "{\"ResultType\" : \"success1\"}";
                //Result = JObject.Parse(r.ToString());
            }
            catch (Exception)
            {
                string nullerror = "{\"ResultType\" : \"error1\"}";
                Result = JObject.Parse(nullerror.ToString());

            }

            return Result;
        }


        // GET: Reasoner
        //Buildingの検索
        [HttpPost]
        [Route("api/Virtuoso/ReasonerBuilding")]
        public JObject Building([FromBody] graphURI graph)
        {


            JObject Result;
            try
            {
                //準備
                StringBuilder Resultstring = new StringBuilder();
                Resultstring.AppendLine("{");
                

                Graph g = new Graph { BaseUri = new Uri(graph.URIDic["sameas"]) };

                string querystring = "";
                VirtuosoManager virtuoso = new VirtuosoManager("localhost", 1111, "DB", "dba", "0443Kou10");

                //prefixを作成
                foreach (string i in graph.URIDic.Keys)
                {
                    querystring += "prefix " + i + ":<" + graph.URIDic[i] + ">";
                }

                //  Buildingのクエリと代替
                string query = querystring + "CONSTRUCT{?a rdf:type bot:Building.}{graph <" + graph.URIDic["inst1"] + "> {?a rdf:type bot:Building.}}";
                object queryresult = virtuoso.Query(query);
                IGraph g_class_query = (IGraph)queryresult;
                //各グラフにあるビル
                Resultstring.AppendLine("\"ResultType\":");

                //識別最下層があるとき
                if (graph.Identitykey != "")
                {
                    Resultstring.Append("\"success\",");
                    Resultstring.AppendLine("\"Variables\":{");
                    int temp_num1 = 1;
                    foreach (Triple t in g_class_query.Triples)
                    {
                        Resultstring.AppendLine("\"" + t.Subject.ToString() + "\":[");
                        INode Building_URI = t.Subject;
                        string query_buil;
                        if (graph.Start_Phase == "Building")
                        {
                            query_buil = querystring + "CONSTRUCT{?a rdf:type bot:Building.}{graph<" + graph.graph1 + ">{<" + Building_URI + "> <" + graph.Identitykey + "> ?c.}{<" + Building_URI + "> rdf:type ?s.?a rdf:type ?s.}graph<" + graph.graph2 + ">{?a <" + graph.Identitykey + "> ?c.}graph<" + graph.URIDic["inst2"] + ">{?a rdf:type bot:Building.}FILTER (?s != bot:Building) }";

                        }
                        else
                        {
                            query_buil = querystring + "CONSTRUCT{?f <" + graph.Identitykey + "> ?z}{{?d ?p <" + Building_URI + ">.?d rdf:type ?x.?d owl:sameAs ?e.?e ?j ?f.<" + Building_URI + "> rdf:type ?q.<" + Building_URI + "> <" + graph.Identitykey + "> ?z}graph<" + graph.URIDic["inst2"] + ">{?f rdf:type ?q.?f rdf:type bot:Building.}{?f <" + graph.Identitykey + "> ?z.}FILTER (?q != bot:Building)FILTER (?p = bot:hasBuilding || ?p = bot:intersectsZone || ?p = bot:adjacentZone || ?p = bot:containsZone )FILTER (?j = bot:hasBuilding || ?j = bot:intersectsZone || ?j = bot:adjacentZone || ?j = bot:containsZone )FILTER (?x = bot:Site)}";

                        }
                        object queryresult_buil = virtuoso.Query(query_buil);
                        IGraph g_class_query_buil = (IGraph)queryresult_buil;
                        if (g_class_query_buil.IsEmpty == false)
                        {
                            int temp_num = 1;
                            foreach (Triple sameas in g_class_query_buil.Triples)
                            {
                                IUriNode subject_temp = g.CreateUriNode(UriFactory.Create(t.Subject.ToString()));
                                IUriNode predicate_temp = g.CreateUriNode(UriFactory.Create(graph.URIDic["owl"] + "sameAs"));
                                IUriNode object_temp = g.CreateUriNode(UriFactory.Create(sameas.Subject.ToString()));
                                g.Assert(subject_temp, predicate_temp, object_temp);
                                if (temp_num1 < g_class_query.Triples.Count())
                                {
                                    if (temp_num < g_class_query_buil.Triples.Count())
                                    {
                                        Resultstring.AppendLine("\"" + sameas.Subject.ToString() + "\",");
                                        temp_num++;
                                    }
                                    else
                                    {
                                        Resultstring.AppendLine("\"" + sameas.Subject.ToString() + "\"],");
                                        temp_num1++;
                                    }
                                }
                                else
                                {
                                    if (temp_num < g_class_query_buil.Triples.Count())
                                    {
                                        Resultstring.AppendLine("\"" + sameas.Subject.ToString() + "\",");
                                        temp_num++;
                                    }
                                    else
                                    {
                                        Resultstring.AppendLine("\"" + sameas.Subject.ToString() + "\"]");
                                        temp_num1++;
                                    }
                                }
                                
                            }

                        }
                        else
                        {
                            if (temp_num1 < g_class_query.Triples.Count())
                            {
                                Resultstring.AppendLine("],");
                                temp_num1++;
                            }
                            else
                            {
                                Resultstring.AppendLine("]");
                                temp_num1++;
                            }
                                
                        }

                    }
                    Resultstring.AppendLine("}");
                    virtuoso.SaveGraph(g);
                }

                //識別最下層がないとき
                else
                {
                    Resultstring.Append("\"select\",");
                    Resultstring.AppendLine("\"Variables\":{");
                    int temp_num1 = 1;
                    foreach (Triple t in g_class_query.Triples)
                    {
                        Resultstring.AppendLine("\"" + t.Subject.ToString() + "\":[");
                        INode Building_URI = t.Subject;
                        string query_buil;
                        if (graph.Start_Phase == "Building")
                        {
                            query_buil = querystring + "construct{?a rdf:type bot:Building.}{{<" + Building_URI + "> rdf:type ?s.?a rdf:type ?s.}graph<" + graph.URIDic["inst2"] + ">{?a rdf:type bot:Building.}FILTER (?s != bot:Building)}";
                        }
                        else
                        {
                            query_buil = querystring + "CONSTRUCT{?f rdf:type bot:Building.}{{?d ?p <" + Building_URI + ">.?d rdf:type ?x.?d owl:sameAs ?e.?e ?j ?f.<" + Building_URI + "> rdf:type ?q.}graph<" + graph.URIDic["inst2"] + ">{?f rdf:type ?q.?f rdf:type bot:Building.}FILTER (?q != bot:Building)FILTER (?p = bot:hasBuilding || ?p = bot:intersectsZone || ?p = bot:adjacentZone || ?p = bot:containsZone )FILTER (?j = bot:hasBuilding || ?j = bot:intersectsZone || ?j = bot:adjacentZone || ?j = bot:containsZone )FILTER (?x = bot:Site)}";
                        }
                        object queryresult_buil = virtuoso.Query(query_buil);
                        IGraph g_class_query_buil = (IGraph)queryresult_buil;
                        if (g_class_query_buil.IsEmpty == false)
                        {
                            int temp_num = 1;
                            foreach (Triple sameas in g_class_query_buil.Triples)
                            {

                                if (temp_num1 < g_class_query.Triples.Count())
                                {
                                    if (temp_num < g_class_query_buil.Triples.Count())
                                    {
                                        Resultstring.AppendLine("\"" + sameas.Subject.ToString() + "\",");
                                        temp_num++;
                                    }
                                    else
                                    {
                                        Resultstring.AppendLine("\"" + sameas.Subject.ToString() + "\"],");
                                        temp_num1++;
                                    }
                                }
                                else
                                {
                                    if (temp_num < g_class_query_buil.Triples.Count())
                                    {
                                        Resultstring.AppendLine("\"" + sameas.Subject.ToString() + "\",");
                                        temp_num++;
                                    }
                                    else
                                    {
                                        Resultstring.AppendLine("\"" + sameas.Subject.ToString() + "\"]");
                                        temp_num1++;
                                    }
                                }
                            }

                        }
                        else
                        {
                            if (temp_num1 < g_class_query.Triples.Count())
                            {
                                Resultstring.AppendLine("],");
                                temp_num1++;
                            }
                            else
                            {
                                Resultstring.AppendLine("]");
                                temp_num1++;
                            }
                        }

                    }
                    Resultstring.AppendLine("}");
                }
                Resultstring.AppendLine("}");
                Result = JObject.Parse(Resultstring.ToString());


                //string r = "{\"ResultType\" : \"success1\"}";
                //Result = JObject.Parse(r.ToString());
            }
            catch (Exception)
            {
                string nullerror = "{\"ResultType\" : \"error1\"}";
                Result = JObject.Parse(nullerror.ToString());

            }

            return Result;
        }


        // GET: Reasoner
        //Storeyの検索
        [HttpPost]
        [Route("api/Virtuoso/ReasonerStorey")]
        public JObject Storey([FromBody] graphURI graph)
        {


            JObject Result;
            try
            {
                //準備
                StringBuilder Resultstring = new StringBuilder();
                Resultstring.AppendLine("{");


                Graph g = new Graph { BaseUri = new Uri(graph.URIDic["sameas"]) };


                string querystring = "";
                VirtuosoManager virtuoso = new VirtuosoManager("localhost", 1111, "DB", "dba", "0443Kou10");
                virtuoso.LoadGraph(g, graph.URIDic["sameas"]);

                //prefixを作成
                foreach (string i in graph.URIDic.Keys)
                {
                    querystring += "prefix " + i + ":<" + graph.URIDic[i] + ">";
                }

                //  Storeyのクエリと代替
                
                string query = querystring + "CONSTRUCT{?a rdf:type bot:Storey.}{graph <" + graph.URIDic["inst1"] + "> {?a rdf:type bot:Storey.}}";
                object queryresult = virtuoso.Query(query);
                IGraph g_class_query = (IGraph)queryresult;
                //各グラフにあるビル
                Resultstring.AppendLine("\"ResultType\":");

                //識別最下層があるとき
                if (graph.Identitykey != "")
                {
                    int num_storey_type = 0;
                    int num_storey_lev = 0;
                    //storeyタイプがあるかを確認
                    foreach (Triple t in g_class_query.Triples)
                    {
                        if (t.Subject.ToString().Contains("type")==false)
                        {
                            num_storey_lev++;
                            string comfirm_query = querystring + "CONSTRUCT{<" + t.Subject.ToString() + "> rdf:type ?a.}{<" + t.Subject.ToString() + "> rdf:type ?a.}";
                            object comfirm_queryresult = virtuoso.Query(comfirm_query);
                            IGraph g_comfirm_query = (IGraph)comfirm_queryresult;
                            foreach (Triple p in g_comfirm_query.Triples)
                            {
                                if (p.Object.ToString().Contains("type") == true)
                                {
                                    num_storey_type++;
                                }
                            }
                        }                        
                    }


                    Resultstring.Append("\"success\",");
                    Resultstring.AppendLine("\"Variables\":{");
                    int num_s = 0;
                    foreach (Triple t in g_class_query.Triples)
                    {
                        if (t.Subject.ToString().Contains("type") == false)
                        {
                            Resultstring.AppendLine("\"" + t.Subject.ToString() + "\":[");
                            INode Storey_URI = t.Subject;
                            
                            string query_storey ;
                            if (graph.Start_Phase == "Storey")
                            {
                                if (num_storey_type != 0)
                                {
                                    query_storey = querystring + "CONSTRUCT{?f rdf:type ?c.}{graph<" + graph.URIDic["inst1"] + ">{<" + Storey_URI + "> rdf:type ?c.}graph<" + graph.URIDic["inst2"] + ">{?f rdf:type ?c.}FILTER (?c != bot:Storey)}";
                                }
                                else
                                {
                                    query_storey = querystring + "CONSTRUCT{?f rdf:type bot:Storey.}{graph<" + graph.graph1 + ">{<" + Storey_URI + "> <" + graph.Identitykey + "> ?c.}graph<" + graph.graph2 + ">{?f <" + graph.Identitykey + "> ?c.}graph<" + graph.URIDic["inst2"] + ">{?a rdf:type bot:Storey.}}";
                                }
                            }
                            else
                            {
                                if (num_storey_type != 0)
                                {
                                    query_storey = querystring + "CONSTRUCT{?f rdf:type ?c.}{graph<" + graph.URIDic["inst1"] + ">{<" + Storey_URI + "> rdf:type ?c.}{?d ?p <" + Storey_URI + ">.?d rdf:type ?x.?d owl:sameAs ?e.?e ?j ?f.}graph<" + graph.URIDic["inst2"] + ">{?f rdf:type ?c.}FILTER (?c != bot:Storey)FILTER (?p = bot:hasStorey || ?p = bot:containsZone )FILTER (?j = bot:hasStorey || ?j = bot:containsZone )FILTER (?x = bot:Building || ?x = bot:Site)}";
                                }
                                else
                                {
                                    query_storey = querystring + "CONSTRUCT{?f rdf:type bot:Storey.}{graph<" + graph.graph1 + ">{<" + Storey_URI + "> <" + graph.Identitykey + "> ?c.}{?d ?p <" + Storey_URI + ">.?d rdf:type ?x.?d owl:sameAs ?e.?e ?j ?f.}graph<" + graph.graph2 + ">{?f <" + graph.Identitykey + "> ?c.}graph<" + graph.URIDic["inst2"] + ">{?a rdf:type bot:Storey.}FILTER (?p = bot:hasStorey || ?p = bot:containsZone )FILTER (?j = bot:hasStorey || ?j = bot:containsZone )FILTER (?x = bot:Building || ?x = bot:Site)}";
                                }
                            }
                            
                            object queryresult_storey = virtuoso.Query(query_storey);
                            IGraph g_class_query_storey = (IGraph)queryresult_storey;
                            if (g_class_query_storey.IsEmpty == false)
                            {
                               int temp_num = 1;
                               foreach (Triple sameas in g_class_query_storey.Triples)
                               {
                                    IUriNode subject_temp = g.CreateUriNode(UriFactory.Create(t.Subject.ToString()));
                                    IUriNode predicate_temp = g.CreateUriNode(UriFactory.Create(graph.URIDic["owl"] + "sameAs"));
                                    IUriNode object_temp = g.CreateUriNode(UriFactory.Create(sameas.Subject.ToString()));
                                    g.Assert(subject_temp, predicate_temp, object_temp);
                                    if (num_s < num_storey_lev)
                                    {
                                        if (temp_num < g_class_query_storey.Triples.Count())
                                        {
                                            Resultstring.AppendLine("\"" + sameas.Subject.ToString() + "\",");
                                        }
                                        else
                                        {
                                            Resultstring.AppendLine("\"" + sameas.Subject.ToString() + "\"],");
                                        }
                                    }
                                    else
                                    {
                                        if (temp_num < g_class_query_storey.Triples.Count())
                                        {
                                            Resultstring.AppendLine("\"" + sameas.Subject.ToString() + "\",");
                                        }
                                        else
                                        {
                                            Resultstring.AppendLine("\"" + sameas.Subject.ToString() + "\"]");
                                        }
                                    }
                                }

                                }
                            else
                            {

                                if (num_s < num_storey_lev)
                                {
                                    Resultstring.AppendLine("],");
                                }
                                else
                                {
                                    Resultstring.AppendLine("]");
                                }
                            }
                            num_s++;
                        }
                    }
                    Resultstring.AppendLine("}");
                    virtuoso.SaveGraph(g);
                    
                }

                //識別最下層がないとき
                else
                {
                    int num_storey_type = 0;
                    int num_storey_lev = 0;
                    //storeyタイプがあるかを確認
                    foreach (Triple t in g_class_query.Triples)
                    {
                        if (t.Subject.ToString().Contains("type") == false)
                        {
                            num_storey_lev++;
                            string comfirm_query = querystring + "CONSTRUCT{<" + t.Subject.ToString() + "> rdf:type ?a.}{<" + t.Subject.ToString() + "> rdf:type ?a.}";
                            object comfirm_queryresult = virtuoso.Query(comfirm_query);
                            IGraph g_comfirm_query = (IGraph)comfirm_queryresult;
                            foreach (Triple p in g_comfirm_query.Triples)
                            {
                                if (p.Object.ToString().Contains("type") == true)
                                {
                                    num_storey_type++;
                                }
                            }
                        }
                    }


                    Resultstring.Append("\"select\",");
                    Resultstring.AppendLine("\"Variables\":{");
                    int num_s = 0;
                    foreach (Triple t in g_class_query.Triples)
                    {
                        if (t.Subject.ToString().Contains("type") == false)
                        {
                            Resultstring.AppendLine("\"" + t.Subject.ToString() + "\":[");
                            INode Storey_URI = t.Subject;

                            string query_storey = "";
                            if (graph.Start_Phase == "Storey")
                            {
                                if (num_storey_type != 0)
                                {
                                    query_storey = querystring + "CONSTRUCT{?f rdf:type ?c.}{graph<" + graph.URIDic["inst1"] + ">{<" + Storey_URI + "> rdf:type ?c.}graph<" + graph.URIDic["inst2"] + ">{?f rdf:type ?c.}FILTER (?c != bot:Storey)}";
                                }
                                else
                                {
                                    query_storey = querystring + "construct{?f rdf:type bot:Storey.}{graph<" + graph.URIDic["inst2"] + ">{?a rdf:type bot:Storey.}";
                                }
                            }
                            else
                            {
                                if (num_storey_type != 0)
                                {
                                    query_storey = querystring + "CONSTRUCT{?f rdf:type ?c.}{graph<" + graph.URIDic["inst1"] + ">{<" + Storey_URI + "> rdf:type ?c.}{?d ?p <" + Storey_URI + ">.?d rdf:type ?x.?d owl:sameAs ?e.?e ?j ?f.}graph<" + graph.URIDic["inst2"] + ">{?f rdf:type ?c.}FILTER (?c != bot:Storey)FILTER (?p = bot:hasStorey || ?p = bot:containsZone )FILTER (?j = bot:hasStorey || ?j = bot:containsZone )FILTER (?x = bot:Building || ?x = bot:Site)}";
                                }
                                else
                                {
                                    query_storey = querystring + "construct{?f rdf:type bot:Storey.}{?d ?p <" + Storey_URI + ">.?d rdf:type ?x.?d owl:sameAs ?e.?e ?j ?f.}{graph<" + graph.URIDic["inst2"] + ">{?a rdf:type bot:Storey.}FILTER (?p = bot:hasStorey || ?p = bot:containsZone )FILTER (?j = bot:hasStorey || ?j = bot:containsZone )FILTER (?x = bot:Building || ?x = bot:Site)}";
                                }
                            }
                            
                            object queryresult_storey = virtuoso.Query(query_storey);
                            IGraph g_class_query_storey = (IGraph)queryresult_storey;
                            if (g_class_query_storey.IsEmpty == false)
                            {
                                int temp_num = 1;
                                foreach (Triple sameas in g_class_query_storey.Triples)
                                {
                                    if (num_s < num_storey_lev)
                                    {
                                        if (temp_num < g_class_query_storey.Triples.Count())
                                        {
                                            Resultstring.AppendLine("\"" + sameas.Subject.ToString() + "\",");
                                        }
                                        else
                                        {
                                            Resultstring.AppendLine("\"" + sameas.Subject.ToString() + "\"],");
                                        }
                                    }
                                    else
                                    {
                                        if (temp_num < g_class_query_storey.Triples.Count())
                                        {
                                            Resultstring.AppendLine("\"" + sameas.Subject.ToString() + "\",");
                                        }
                                        else
                                        {
                                            Resultstring.AppendLine("\"" + sameas.Subject.ToString() + "\"]");
                                        }
                                    }
                                }

                            }
                            else
                            {
                                if (num_s < num_storey_lev)
                                {
                                    Resultstring.AppendLine("],");
                                }
                                else
                                {
                                    Resultstring.AppendLine("]");
                                }
                            }
                            num_s++;
                        }
                    }
                    Resultstring.AppendLine("}");
                }
                Resultstring.AppendLine("}");
                Result = JObject.Parse(Resultstring.ToString());


                //string r = "{\"ResultType\" : \"success1\"}";
                //Result = JObject.Parse(r.ToString());
            }
            catch (Exception)
            {
                string nullerror = "{\"ResultType\" : \"error1\"}";
                Result = JObject.Parse(nullerror.ToString());

            }

            return Result;
        }


        //Spaceの検索
        [HttpPost]
        [Route("api/Virtuoso/ReasonerSpace")]
        public JObject Space([FromBody] graphURI graph)
        {


            JObject Result;
            try
            {
                //準備
                StringBuilder Resultstring = new StringBuilder();
                Resultstring.AppendLine("{");


                Graph g = new Graph { BaseUri = new Uri(graph.URIDic["sameas"]) };

                string querystring = "";
                VirtuosoManager virtuoso = new VirtuosoManager("localhost", 1111, "DB", "dba", "0443Kou10");
                virtuoso.LoadGraph(g, graph.URIDic["sameas"]);

                //prefixを作成
                foreach (string i in graph.URIDic.Keys)
                {
                    querystring += "prefix " + i + ":<" + graph.URIDic[i] + ">";
                }

                //  Spaceのクエリと代替
                string query = querystring + "CONSTRUCT{?a rdf:type bot:Space.}{graph <" + graph.URIDic["inst1"] + "> {?a rdf:type bot:Space.}}";
                object queryresult = virtuoso.Query(query);
                IGraph g_class_query = (IGraph)queryresult;
                //各グラフにあるビル
                Resultstring.AppendLine("\"ResultType\":");

                //識別最下層があるとき
                if (graph.Identitykey != "")
                {
                    Resultstring.Append("\"success\",");
                    Resultstring.AppendLine("\"Variables\":{");
                    int temp_num1 = 1;
                    foreach (Triple t in g_class_query.Triples)
                    {
                        Resultstring.AppendLine("\"" + t.Subject.ToString() + "\":[");
                        INode Space_URI = t.Subject;
                        string query_buil;
                        if (graph.Start_Phase == "Space")
                        {
                            query_buil = querystring + "CONSTRUCT{?f <" + graph.Identitykey + "> ?z}{{<" + Space_URI + "> rdf:type ?q.<" + Space_URI + "> <" + graph.Identitykey + "> ?z}graph<" + graph.URIDic["inst2"] + ">{?f rdf:type ?q.?f rdf:type bot:Space.}{?f <" + graph.Identitykey + "> ?z.}FILTER (?q != bot:Space)}";

                        }
                        else
                        {
                            query_buil = querystring + "CONSTRUCT{?f <" + graph.Identitykey + "> ?z}{{?d ?p <" + Space_URI + ">.?d rdf:type ?x.?d owl:sameAs ?e.?e ?j ?f.<" + Space_URI + "> rdf:type ?q.<" + Space_URI + "> <" + graph.Identitykey + "> ?z}graph<" + graph.URIDic["inst2"] + ">{?f rdf:type ?q.?f rdf:type bot:Space.}{?f <" + graph.Identitykey + "> ?z.}FILTER (?q != bot:Space)FILTER (?p = bot:hasSpace || ?p = bot:intersectsZone || ?p = bot:adjacentZone || ?p = bot:containsZone )FILTER (?x = bot:Storey || ?x = bot:Building)FILTER (?j = bot:hasSpace || ?j = bot:intersectsZone || ?j = bot:adjacentZone || ?j = bot:containsZone )}";

                        }
                        object queryresult_buil = virtuoso.Query(query_buil);
                        IGraph g_class_query_buil = (IGraph)queryresult_buil;
                        if (g_class_query_buil.IsEmpty == false)
                        {
                            int temp_num = 1;
                            foreach (Triple sameas in g_class_query_buil.Triples)
                            {
                                IUriNode subject_temp = g.CreateUriNode(UriFactory.Create(t.Subject.ToString()));
                                IUriNode predicate_temp = g.CreateUriNode(UriFactory.Create(graph.URIDic["owl"] + "sameAs"));
                                IUriNode object_temp = g.CreateUriNode(UriFactory.Create(sameas.Subject.ToString()));
                                g.Assert(subject_temp, predicate_temp, object_temp);
                                if (temp_num1 < g_class_query.Triples.Count())
                                {
                                    if (temp_num < g_class_query_buil.Triples.Count())
                                    {
                                        Resultstring.AppendLine("\"" + sameas.Subject.ToString() + "\",");
                                        temp_num++;
                                    }
                                    else
                                    {
                                        Resultstring.AppendLine("\"" + sameas.Subject.ToString() + "\"],");
                                        temp_num1++;
                                    }
                                }
                                else
                                {
                                    if (temp_num < g_class_query_buil.Triples.Count())
                                    {
                                        Resultstring.AppendLine("\"" + sameas.Subject.ToString() + "\",");
                                        temp_num++;
                                    }
                                    else
                                    {
                                        Resultstring.AppendLine("\"" + sameas.Subject.ToString() + "\"]");
                                        temp_num1++;
                                    }
                                }

                            }

                        }
                        else
                        {
                            if (temp_num1 < g_class_query.Triples.Count())
                            {
                                Resultstring.AppendLine("],");
                                temp_num1++;
                            }
                            else
                            {
                                Resultstring.AppendLine("]");
                                temp_num1++;
                            }

                        }

                    }
                    Resultstring.AppendLine("}");
                    virtuoso.SaveGraph(g);
                }

                //識別最下層がないとき
                else
                {
                    Resultstring.Append("\"select\",");
                    Resultstring.AppendLine("\"Variables\":{");
                    int temp_num1 = 1;
                    foreach (Triple t in g_class_query.Triples)
                    {
                        Resultstring.AppendLine("\"" + t.Subject.ToString() + "\":[");
                        INode Space_URI = t.Subject;
                        string query_buil;
                        if (graph.Start_Phase == "Space")
                        {
                            query_buil = querystring + "CONSTRUCT{?f rdf:type bot:Space.}{{<" + Space_URI + "> rdf:type ?q.}graph<" + graph.URIDic["inst2"] + ">{?f rdf:type ?q.?f rdf:type bot:Space.}FILTER (?q != bot:Space)}";

                        }
                        else
                        {
                            query_buil = querystring + "CONSTRUCT{?f rdf:type bot:Space.}{{?d ?p <" + Space_URI + ">.?d rdf:type ?x.?d owl:sameAs ?e.?e ?j ?f.<" + Space_URI + "> rdf:type ?q.}graph<" + graph.URIDic["inst2"] + ">{?f rdf:type ?q.?f rdf:type bot:Space.}FILTER (?q != bot:Space)FILTER (?p = bot:hasSpace || ?p = bot:intersectsZone || ?p = bot:adjacentZone || ?p = bot:containsZone )FILTER (?j = bot:hasSpace || ?j = bot:intersectsZone || ?j = bot:adjacentZone || ?j = bot:containsZone )FILTER (?x = bot:Storey || ?x = bot:Building)}";

                        }
                        object queryresult_buil = virtuoso.Query(query_buil);
                        IGraph g_class_query_buil = (IGraph)queryresult_buil;
                        if (g_class_query_buil.IsEmpty == false)
                        {
                            int temp_num = 1;
                            foreach (Triple sameas in g_class_query_buil.Triples)
                            {

                                if (temp_num1 < g_class_query.Triples.Count())
                                {
                                    if (temp_num < g_class_query_buil.Triples.Count())
                                    {
                                        Resultstring.AppendLine("\"" + sameas.Subject.ToString() + "\",");
                                        temp_num++;
                                    }
                                    else
                                    {
                                        Resultstring.AppendLine("\"" + sameas.Subject.ToString() + "\"],");
                                        temp_num1++;
                                    }
                                }
                                else
                                {
                                    if (temp_num < g_class_query_buil.Triples.Count())
                                    {
                                        Resultstring.AppendLine("\"" + sameas.Subject.ToString() + "\",");
                                        temp_num++;
                                    }
                                    else
                                    {
                                        Resultstring.AppendLine("\"" + sameas.Subject.ToString() + "\"]");
                                        temp_num1++;
                                    }
                                }
                            }

                        }
                        else
                        {
                            if (temp_num1 < g_class_query.Triples.Count())
                            {
                                Resultstring.AppendLine("],");
                                temp_num1++;
                            }
                            else
                            {
                                Resultstring.AppendLine("]");
                                temp_num1++;
                            }
                        }

                    }
                    Resultstring.AppendLine("}");
                }
                Resultstring.AppendLine("}");
                Result = JObject.Parse(Resultstring.ToString());


                //string r = "{\"ResultType\" : \"success1\"}";
                //Result = JObject.Parse(r.ToString());
            }
            catch (Exception)
            {
                string nullerror = "{\"ResultType\" : \"error1\"}";
                Result = JObject.Parse(nullerror.ToString());

            }

            return Result;
        }


        //Elementの検索
        [HttpPost]
        [Route("api/Virtuoso/ReasonerElement")]
        public JObject Element([FromBody] graphURI graph)
        {


            JObject Result;
            try
            {
                //準備
                StringBuilder Resultstring = new StringBuilder();
                Resultstring.AppendLine("{");


                Graph g = new Graph { BaseUri = new Uri(graph.URIDic["sameas"]) };

                string querystring = "";
                VirtuosoManager virtuoso = new VirtuosoManager("localhost", 1111, "DB", "dba", "0443Kou10");
                virtuoso.LoadGraph(g, graph.URIDic["sameas"]);

                //prefixを作成
                foreach (string i in graph.URIDic.Keys)
                {
                    querystring += "prefix " + i + ":<" + graph.URIDic[i] + ">";
                }

                //  Spaceのクエリと代替
                string query = querystring + "CONSTRUCT{?a rdf:type bot:Element.}{graph <" + graph.URIDic["inst1"] + "> {?a rdf:type bot:Element.}}";
                object queryresult = virtuoso.Query(query);
                IGraph g_class_query = (IGraph)queryresult;
                //各グラフにあるビル
                Resultstring.AppendLine("\"ResultType\":");

                //識別最下層があるとき
                if (graph.Identitykey != "")
                {
                    Resultstring.Append("\"success\",");
                    Resultstring.AppendLine("\"Variables\":{");
                    int temp_num1 = 1;
                    foreach (Triple t in g_class_query.Triples)
                    {
                        Resultstring.AppendLine("\"" + t.Subject.ToString() + "\":[");
                        INode Element_URI = t.Subject;
                        string query_buil;
                        if (graph.Start_Phase == "Element")
                        {
                            query_buil = querystring + "CONSTRUCT{?f rdf:type bot:Element.}{{<" + Element_URI + "> type:Typeof ?h.?h rdf:type ?q.<" + Element_URI + "> <" + graph.Identitykey + "> ?z.}graph<" + graph.graph2 + ">{?f <" + graph.Identitykey + "> ?z.}graph<" + graph.URIDic["inst2"] + ">{?t rdf:type ?q.?f type:Typeof ?l.?f rdf:type bot:Element.}FILTER (?q != type:Type)}";

                        }
                        else
                        {
                            query_buil = querystring + "CONSTRUCT{?f rdf:type bot:Element.}{{?d ?p <" + Element_URI + ">.?d rdf:type ?x.?d owl:sameAs ?e.?e ?j ?f.<" + Element_URI + "> type:Typeof ?h.?h rdf:type ?q.?h owl:sameAs ?l.<" + Element_URI + "> <" + graph.Identitykey + "> ?z.}graph<" + graph.graph2 + ">{?f <" + graph.Identitykey + "> ?z.}graph<" + graph.URIDic["inst2"] + ">{?t rdf:type ?q.?f type:Typeof ?l.?f rdf:type bot:Element.}FILTER (?q != type:Type)FILTER (?x = bot:Storey || ?x = bot:Space || ?x = bot:Building || ?x = bot:Site)FILTER(?p = bot:hasElement || ?p = bot:intersectingElement || ?p = bot:adjacentElement || ?p = bot:containsElement )FILTER (?j = bot:hasElement || ?j = bot:intersectingElement || ?j = bot:adjacentElement || ?j = bot:containsElement )}";

                        }
                        object queryresult_buil = virtuoso.Query(query_buil);
                        IGraph g_class_query_buil = (IGraph)queryresult_buil;
                        if (g_class_query_buil.IsEmpty == false)
                        {
                            int temp_num = 1;
                            foreach (Triple sameas in g_class_query_buil.Triples)
                            {
                                IUriNode subject_temp = g.CreateUriNode(UriFactory.Create(t.Subject.ToString()));
                                IUriNode predicate_temp = g.CreateUriNode(UriFactory.Create(graph.URIDic["owl"] + "sameAs"));
                                IUriNode object_temp = g.CreateUriNode(UriFactory.Create(sameas.Subject.ToString()));
                                g.Assert(subject_temp, predicate_temp, object_temp);
                                if (temp_num1 < g_class_query.Triples.Count())
                                {
                                    if (temp_num < g_class_query_buil.Triples.Count())
                                    {
                                        Resultstring.AppendLine("\"" + sameas.Subject.ToString() + "\",");
                                        temp_num++;
                                    }
                                    else
                                    {
                                        Resultstring.AppendLine("\"" + sameas.Subject.ToString() + "\"],");
                                        temp_num1++;
                                    }
                                }
                                else
                                {
                                    if (temp_num < g_class_query_buil.Triples.Count())
                                    {
                                        Resultstring.AppendLine("\"" + sameas.Subject.ToString() + "\",");
                                        temp_num++;
                                    }
                                    else
                                    {
                                        Resultstring.AppendLine("\"" + sameas.Subject.ToString() + "\"]");
                                        temp_num1++;
                                    }
                                }

                            }

                        }
                        else
                        {
                            if (temp_num1 < g_class_query.Triples.Count())
                            {
                                Resultstring.AppendLine("],");
                                temp_num1++;
                            }
                            else
                            {
                                Resultstring.AppendLine("]");
                                temp_num1++;
                            }

                        }

                    }
                    Resultstring.AppendLine("}");
                    virtuoso.SaveGraph(g);
                }

                //識別最下層がないとき
                else
                {
                    Resultstring.Append("\"select\",");
                    Resultstring.AppendLine("\"Variables\":{");
                    int temp_num1 = 1;
                    foreach (Triple t in g_class_query.Triples)
                    {
                        Resultstring.AppendLine("\"" + t.Subject.ToString() + "\":[");
                        INode Element_URI = t.Subject;
                        string query_buil = querystring + "CONSTRUCT{?f rdf:type bot:Element.}{{?d ?p <" + Element_URI + ">.?d rdf:type ?x.?d owl:sameAs ?e.?e ?j ?f.<" + Element_URI + "> type:Typeof ?h.?h rdf:type ?q.?h owl:sameAs ?l.}graph<" + graph.URIDic["inst2"] + ">{?t rdf:type ?q.?f type:Typeof ?l.?f rdf:type bot:Element.}FILTER (?q != type:Type)FILTER (?x = bot:Space || ?x = bot:Building || ?x = bot:Site)FILTER (?p = bot:hasElement || ?p = bot:intersectingElement || ?p = bot:adjacentElement || ?p = bot:containsElement )FILTER (?j = bot:hasElement || ?j = bot:intersectingElement || ?j = bot:adjacentElement || ?j = bot:containsElement )}";

                        object queryresult_buil = virtuoso.Query(query_buil);
                        IGraph g_class_query_buil = (IGraph)queryresult_buil;
                        if (g_class_query_buil.IsEmpty == true)
                        {
                            query_buil = querystring + "CONSTRUCT{?f rdf:type bot:Element.}{{?d ?p <" + Element_URI + ">.?d rdf:type ?x.?d owl:sameAs ?e.?e ?j ?f.<" + Element_URI + "> type:Typeof ?h.?h rdf:type ?q.?h owl:sameAs ?l.}graph<" + graph.URIDic["inst2"] + ">{?t rdf:type ?q.?f type:Typeof ?l.?f rdf:type bot:Element.}FILTER (?q != type:Type)FILTER (?x = bot:Storey || ?x = bot:Space || ?x = bot:Building || ?x = bot:Site)FILTER (?p = bot:hasElement || ?p = bot:intersectingElement || ?p = bot:adjacentElement || ?p = bot:containsElement )FILTER (?j = bot:hasElement || ?j = bot:intersectingElement || ?j = bot:adjacentElement || ?j = bot:containsElement )}";

                            queryresult_buil = virtuoso.Query(query_buil);
                            g_class_query_buil = (IGraph)queryresult_buil;
                        }
                        if (g_class_query_buil.IsEmpty == false)
                        {
                            int temp_num = 1;
                            foreach (Triple sameas in g_class_query_buil.Triples)
                            {

                                if (temp_num1 < g_class_query.Triples.Count())
                                {
                                    if (temp_num < g_class_query_buil.Triples.Count())
                                    {
                                        Resultstring.AppendLine("\"" + sameas.Subject.ToString() + "\",");
                                        temp_num++;
                                    }
                                    else
                                    {
                                        Resultstring.AppendLine("\"" + sameas.Subject.ToString() + "\"],");
                                        temp_num1++;
                                    }
                                }
                                else
                                {
                                    if (temp_num < g_class_query_buil.Triples.Count())
                                    {
                                        Resultstring.AppendLine("\"" + sameas.Subject.ToString() + "\",");
                                        temp_num++;
                                    }
                                    else
                                    {
                                        Resultstring.AppendLine("\"" + sameas.Subject.ToString() + "\"]");
                                        temp_num1++;
                                    }
                                }
                            }

                        }
                        else
                        {
                            if (temp_num1 < g_class_query.Triples.Count())
                            {
                                Resultstring.AppendLine("],");
                                temp_num1++;
                            }
                            else
                            {
                                Resultstring.AppendLine("]");
                                temp_num1++;
                            }
                        }

                    }
                    Resultstring.AppendLine("}");
                }
                Resultstring.AppendLine("}");
                Result = JObject.Parse(Resultstring.ToString());


                //string r = "{\"ResultType\" : \"success1\"}";
                //Result = JObject.Parse(r.ToString());
            }
            catch (Exception)
            {
                string nullerror = "{\"ResultType\" : \"error1\"}";
                Result = JObject.Parse(nullerror.ToString());

            }

            return Result;
        }

        //Typeの検索
        [HttpPost]
        [Route("api/Virtuoso/ReasonerType")]
        public JObject Type([FromBody] graphURI graph)
        {


            JObject Result;
            try
            {
                //準備
                StringBuilder Resultstring = new StringBuilder();
                Resultstring.AppendLine("{");


                Graph g = new Graph { BaseUri = new Uri(graph.URIDic["sameas"]) };

                string querystring = "";
                VirtuosoManager virtuoso = new VirtuosoManager("localhost", 1111, "DB", "dba", "0443Kou10");
                virtuoso.LoadGraph(g, graph.URIDic["sameas"]);

                //prefixを作成
                foreach (string i in graph.URIDic.Keys)
                {
                    querystring += "prefix " + i + ":<" + graph.URIDic[i] + ">";
                }

                //  Spaceのクエリと代替
                string query = querystring + "CONSTRUCT{?a rdf:type type:Type.}{graph <" + graph.URIDic["inst1"] + "> {?a rdf:type type:Type.}}";
                object queryresult = virtuoso.Query(query);
                IGraph g_class_query = (IGraph)queryresult;
                //各グラフにあるビル
                Resultstring.AppendLine("\"ResultType\":");

                //識別最下層があるとき
                if (graph.Identitykey != "")
                {
                    Resultstring.Append("\"success\",");
                    Resultstring.AppendLine("\"Variables\":{");
                    int temp_num1 = 1;
                    foreach (Triple t in g_class_query.Triples)
                    {
                        Resultstring.AppendLine("\"" + t.Subject.ToString() + "\":[");
                        INode Type_URI = t.Subject;
                        string query_buil;
                        if (graph.Start_Phase == "Type")
                        {
                            query_buil = querystring + "CONSTRUCT{?f rdf:type type:Type.}{{<" + Type_URI + "> rdf:type ?q.<" + Type_URI + "> <" + graph.Identitykey + "> ?z.}graph<" + graph.graph2 + ">{?f <" + graph.Identitykey + "> ?z.}graph<" + graph.URIDic["inst2"] + ">{?f rdf:type ?q.?f rdf:type type:Type.}FILTER (?q != type:Type)}";

                        }
                        else
                        {
                            query_buil = querystring + "CONSTRUCT{?f rdf:type type:Type.}{{?d type:Typeof  <" + Type_URI + ">.?w ?p ?d.?w rdf:type ?x.?w owl:sameAs ?e.?e ?j ?h.?h type:Typeof ?f.<" + Type_URI + "> rdf:type ?q.<" + Type_URI + "> <" + graph.Identitykey + "> ?z.}graph<" + graph.graph2 + ">{?f <" + graph.Identitykey + "> ?z.}graph<" + graph.URIDic["inst2"] + ">{?f rdf:type ?q.?f rdf:type type:Type.}FILTER (?q != type:Type)FILTER (?p = bot:hasElement || ?p = bot:intersectingElement || ?p = bot:adjacentElement || ?p = bot:containsElement )FILTER (?j = bot:hasElement || ?j = bot:intersectingElement || ?j = bot:adjacentElement || ?j = bot:containsElement )FILTER (?x = bot:Storey || ?x = bot:Space || ?x = bot:Building || ?x = bot:Site)}";
                        }
                        object queryresult_buil = virtuoso.Query(query_buil);
                        IGraph g_class_query_buil = (IGraph)queryresult_buil;
                        if (g_class_query_buil.IsEmpty == false)
                        {
                            int temp_num = 1;
                            foreach (Triple sameas in g_class_query_buil.Triples)
                            {
                                IUriNode subject_temp = g.CreateUriNode(UriFactory.Create(t.Subject.ToString()));
                                IUriNode predicate_temp = g.CreateUriNode(UriFactory.Create(graph.URIDic["owl"] + "sameAs"));
                                IUriNode object_temp = g.CreateUriNode(UriFactory.Create(sameas.Subject.ToString()));
                                g.Assert(subject_temp, predicate_temp, object_temp);
                                if (temp_num1 < g_class_query.Triples.Count())
                                {
                                    if (temp_num < g_class_query_buil.Triples.Count())
                                    {
                                        Resultstring.AppendLine("\"" + sameas.Subject.ToString() + "\",");
                                        temp_num++;
                                    }
                                    else
                                    {
                                        Resultstring.AppendLine("\"" + sameas.Subject.ToString() + "\"],");
                                        temp_num1++;
                                    }
                                }
                                else
                                {
                                    if (temp_num < g_class_query_buil.Triples.Count())
                                    {
                                        Resultstring.AppendLine("\"" + sameas.Subject.ToString() + "\",");
                                        temp_num++;
                                    }
                                    else
                                    {
                                        Resultstring.AppendLine("\"" + sameas.Subject.ToString() + "\"]");
                                        temp_num1++;
                                    }
                                }

                            }

                        }
                        else
                        {
                            if (temp_num1 < g_class_query.Triples.Count())
                            {
                                Resultstring.AppendLine("],");
                                temp_num1++;
                            }
                            else
                            {
                                Resultstring.AppendLine("]");
                                temp_num1++;
                            }

                        }

                    }
                    Resultstring.AppendLine("}");
                    virtuoso.SaveGraph(g);
                }

                //識別最下層がないとき
                else
                {
                    Resultstring.Append("\"select\",");
                    Resultstring.AppendLine("\"Variables\":{");
                    int temp_num1 = 1;
                    foreach (Triple t in g_class_query.Triples)
                    {
                        Resultstring.AppendLine("\"" + t.Subject.ToString() + "\":[");
                        INode Type_URI = t.Subject;
                        string query_buil;
                        if (graph.Start_Phase == "Type")
                        {
                            query_buil = querystring + "CONSTRUCT{?f rdf:type type:Type.}{{<" + Type_URI + "> rdf:type ?q.}graph<" + graph.URIDic["inst2"] + ">{?f rdf:type ?q.?f rdf:type type:Type.}FILTER (?q != type:Type)}";

                        }
                        else
                        {
                            query_buil = querystring + "CONSTRUCT{?f rdf:type type:Type.}{{?d type:Typeof  <" + Type_URI + ">.?w ?p ?d.?w rdf:type ?x.?w owl:sameAs ?e.?e ?j ?h.?h type:Typeof ?f.<" + Type_URI + "> rdf:type ?q.}graph<" + graph.URIDic["inst2"] + ">{?f rdf:type ?q.?f rdf:type type:Type.}FILTER (?q != type:Type)FILTER (?p = bot:hasElement || ?p = bot:intersectingElement || ?p = bot:adjacentElement || ?p = bot:containsElement )FILTER (?j = bot:hasElement || ?j = bot:intersectingElement || ?j = bot:adjacentElement || ?j = bot:containsElement )FILTER (?x = bot:Storey || ?x = bot:Space || ?x = bot:Building || ?x = bot:Site)}";
                        }
                        object queryresult_buil = virtuoso.Query(query_buil);
                        IGraph g_class_query_buil = (IGraph)queryresult_buil;
                        if (g_class_query_buil.IsEmpty == false)
                        {
                            int temp_num = 1;
                            foreach (Triple sameas in g_class_query_buil.Triples)
                            {

                                if (temp_num1 < g_class_query.Triples.Count())
                                {
                                    if (temp_num < g_class_query_buil.Triples.Count())
                                    {
                                        Resultstring.AppendLine("\"" + sameas.Subject.ToString() + "\",");
                                        temp_num++;
                                    }
                                    else
                                    {
                                        Resultstring.AppendLine("\"" + sameas.Subject.ToString() + "\"],");
                                        temp_num1++;
                                    }
                                }
                                else
                                {
                                    if (temp_num < g_class_query_buil.Triples.Count())
                                    {
                                        Resultstring.AppendLine("\"" + sameas.Subject.ToString() + "\",");
                                        temp_num++;
                                    }
                                    else
                                    {
                                        Resultstring.AppendLine("\"" + sameas.Subject.ToString() + "\"]");
                                        temp_num1++;
                                    }
                                }
                            }

                        }
                        else
                        {
                            if (temp_num1 < g_class_query.Triples.Count())
                            {
                                Resultstring.AppendLine("],");
                                temp_num1++;
                            }
                            else
                            {
                                Resultstring.AppendLine("]");
                                temp_num1++;
                            }
                        }

                    }
                    Resultstring.AppendLine("}");
                }
                Resultstring.AppendLine("}");
                Result = JObject.Parse(Resultstring.ToString());


                //string r = "{\"ResultType\" : \"success1\"}";
                //Result = JObject.Parse(r.ToString());
            }
            catch (Exception)
            {
                string nullerror = "{\"ResultType\" : \"error1\"}";
                Result = JObject.Parse(nullerror.ToString());

            }

            return Result;
        }

        public class graphURI
        {
            public string graph1 { get; set; }

            public string graph2 { get; set; }

            public string Identitykey { get; set; }

            public string Start_Phase { get; set; }

            public Dictionary<string, string> URIDic = new Dictionary<string, string>()
            {
                {"inst1", "https://www.ugent.be/LBDtemp1#"},
                {"inst2", "https://www.ugent.be/LBDtemp2#"},
                {"sameas", "https://www.ugent.be/sameas#"},
                {"bot", "https://w3id.org/bot#"},
                {"owl", "http://www.w3.org/2002/07/owl#"},
                {"uniclass", "https://koo/uniclass#"},
                {"type", "http://koo/type#"},
                {"rdf", "http://www.w3.org/1999/02/22-rdf-syntax-ns#"}
            };
        }
    }

}