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
using Newtonsoft.Json;

namespace SemanticWebDataCloud.Controllers
{
    public class VIrtuosoController : ApiController
    {

        // GET: VIrtuoso
        //クエリ
        [HttpPost]
        [Route("api/Virtuoso/jobs")]
        public JObject Query([FromBody] QueryObject objModel)
        {
            JObject Result;
            string nullerror = "{\"error\" : \"null\"}";
            Result = JObject.Parse(nullerror.ToString());
            StringBuilder Resultstring = new StringBuilder();
            Resultstring.AppendLine("{");

            if (objModel.Querycode != null)
            {
                VirtuosoManager virtuoso = new VirtuosoManager("localhost", 1111, "DB", "dba", "0443Kou10");
                try
                {
                    //VIrtuosoへのクエリ
                    object graph = virtuoso.Query(objModel.Querycode);



                    //selectのクエリの場合
                    if (graph is SparqlResultSet)
                    {
                        Resultstring.AppendLine("\"ResultType\":\"Select\",");

                        //結果のjsonへの変換
                        SparqlResultSet rset = (SparqlResultSet)graph;
                        IEnumerable<string> keyenum = rset.Variables;
                        List<string> keylist = keyenum.ToList();

                        Resultstring.AppendLine("\"Variables\":[");
                        int indexval = 0;
                        foreach (string key in keylist)
                        {
                            if (indexval < keylist.Count - 1)
                            {
                                Resultstring.AppendLine("\"" + key + "\",");
                                indexval++;
                            }
                            else
                            {
                                Resultstring.AppendLine("\"" + key + "\"],");

                            }
                        }

                        int index1 = 0;
                        foreach (SparqlResult result1 in rset)
                        {
                            int index2 = 0;
                            Resultstring.AppendLine("\"" + index1 + "\":[");
                            foreach (string key in keylist)
                            {
                                if (index2 < keylist.Count - 1)
                                {
                                    Resultstring.AppendLine("\"" + result1[key] + "\",");
                                    index2++;
                                }
                                else
                                {
                                    Resultstring.AppendLine("\"" + result1[key] + "\"");
                                    index2 = 0;
                                }
                            }
                            if (index1 < rset.Count - 1)
                            {
                                Resultstring.AppendLine("],");
                                index1++;
                            }
                            else
                            {
                                Resultstring.AppendLine("]");
                            }


                        }
                        Resultstring.AppendLine("}");

                    



                        Result = JObject.Parse(Resultstring.ToString());
                        //Console.WriteLine(Result);
                        /*
                        int ii = 0;
                        foreach (SparqlResult result1 in rset)
                        {
                            Console.WriteLine(ii + result1.ToString());
                            ii++;

                        }
                        */

                    }


                    //CONSTRUCTのクエリの場合
                    else if (graph is IGraph)
                    {
                        Resultstring.AppendLine("\"ResultType\":\"Construct\",");


                        //結果のjsonへの変換
                        IGraph g = (IGraph)graph;

                        HashSet<INode> sub = new HashSet<INode>();

                        //subjectを抽出
                        foreach (Triple t in g.Triples)
                        {
                            INode subnode = t.Subject;
                            sub.Add(subnode);
                        }



                        int tempj = 1;
                        foreach (INode t in sub)
                        {
                            HashSet<INode> pre = new HashSet<INode>();

                            foreach (Triple j in g.GetTriplesWithSubject(t))
                            {
                                INode prenode = j.Predicate;
                                pre.Add(prenode);
                            }


                            Resultstring.AppendLine("\"" + t.ToString() + "\":{");

                            int tempi = 1;
                            foreach (INode h in pre)
                            {

                                if (g.GetTriplesWithSubjectPredicate(t, h).Count() > 1)
                                {
                                    int tempo = 1;
                                    Resultstring.AppendLine("\"" + h.ToString() + "\":[");
                                    foreach (Triple j in g.GetTriplesWithSubjectPredicate(t, h))
                                    {
                                        if (tempo < g.GetTriplesWithSubjectPredicate(t, h).Count())
                                        {
                                            Resultstring.AppendLine("\"" + j.Object.ToString() + "\",");
                                            tempo++;
                                        }
                                        else
                                        {
                                            Resultstring.AppendLine("\"" + j.Object.ToString() + "\"");
                                        }

                                    }
                                    if (tempi < pre.Count)
                                    {
                                        Resultstring.AppendLine("],");
                                        tempi++;
                                    }
                                    else
                                    {
                                        Resultstring.AppendLine("]");
                                    }

                                }

                                else
                                {
                                    foreach (Triple j in g.GetTriplesWithSubjectPredicate(t, h))
                                    {
                                        if (tempi < pre.Count)
                                        {
                                            Resultstring.AppendLine("\"" + j.Predicate.ToString() + "\":[\"" + j.Object.ToString() + "\"],");
                                            tempi++;
                                        }
                                        else
                                        {
                                            Resultstring.AppendLine("\"" + j.Predicate.ToString() + "\":[\"" + j.Object.ToString() + "\"]");
                                        }
                                    }
                                }
                            }

                            if (tempj < sub.Count)
                            {
                                Resultstring.AppendLine("},");
                                tempj++;
                            }
                            else
                            {
                                Resultstring.AppendLine("}");
                            }

                        }

                        Resultstring.AppendLine(" }");
                        //Console.WriteLine(Resultstring);
                        Result = JObject.Parse(Resultstring.ToString());
                        
                    }
                }
                catch (OpenLink.Data.Virtuoso.VirtuosoException )
                {
                    string error = "{\"ResultType\" : \"QueryError\"}";
                    Result = JObject.Parse(error.ToString());
                    

                }
                catch (Exception )
                {
                    string error = "{\"ResultType\" : \"SystemError\"}";
                    Result = JObject.Parse(error.ToString());
                    
                }

            }
            //何もないときの処理
            else
            {
                string error = "{\"ResultType\" : \"null\"}";
                Result = JObject.Parse(error.ToString());

            }

            return Result;
            
        }

        public class QueryObject
        {
            public string Querycode { get; set; }
        }

        //データをアップロード
        [HttpPost]
        [Route("api/Virtuoso/upload")]      
        public string Upload([FromBody] FileObject uploadfile)
        {
            string result = "";
            VirtuosoManager virtuoso = new VirtuosoManager("localhost", 1111, "DB", "dba", "0443Kou10");
            try
            {
                IGraph g = new Graph();
                
                string file = uploadfile.File;
                StringParser.Parse(g, file);

                virtuoso.SaveGraph(g);
                result = "success";

            }
            catch(VDS.RDF.Storage.RdfStorageException )
            {
                result = "Base Uri が記述されていません";
            }
            catch(Exception)
            {
                result = "error";
            } 
            return result;

          
        }

        public class FileObject
        {
            public string File { get; set; }
        }

        //データアップデート
        [HttpPost]
        [Route("api/Virtuoso/update")]
        public JObject Update([FromBody] updategraph updategraph)
        {
            JObject Result;
            VirtuosoManager virtuoso = new VirtuosoManager("localhost", 1111, "DB", "dba", "0443Kou10");
            
            try
            {
                JObject save_graph = JObject.Parse(updategraph.graph.ToString());
                Graph g = new Graph { BaseUri = new Uri(updategraph.URIDic["sameas"]) };
                if (updategraph.phase != "Building")
                {
                    virtuoso.LoadGraph(g, updategraph.URIDic["sameas"]);
                }
                
                foreach (var i in save_graph)
                {
                    if ((string)i.Value != "")
                    {
                        IUriNode subject_temp = g.CreateUriNode(UriFactory.Create(i.Key));
                        IUriNode predicate_temp = g.CreateUriNode(UriFactory.Create(updategraph.URIDic["owl"] + "sameAs"));
                        IUriNode object_temp = g.CreateUriNode(UriFactory.Create((string)i.Value));
                        g.Assert(subject_temp, predicate_temp, object_temp);
                    }

                    


                }

                virtuoso.SaveGraph(g);




                string success = "{\"ResultType\" : \"success\"}";
                Result = JObject.Parse(success.ToString());


            }
            catch (Exception)
            {
                string nullerror = "{\"ResultType\" : \"error\"}";
                Result = JObject.Parse(nullerror.ToString());

            }

            return Result;
        }

        public class updategraph
        {
            public string graph { get; set; }

            public string phase { get; set; }

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

        //グラフツリーの表示
        [HttpPost]
        [Route("api/Virtuoso/tree")]
        public JObject tree([FromBody] Tree treeobj)
        {
            JObject Result;
            StringBuilder Resultstring = new StringBuilder();
            try
            {
                Resultstring.AppendLine("{\"Result\":[");
                VirtuosoManager virtuoso = new VirtuosoManager("localhost", 1111, "DB", "dba", "0443Kou10");
                object graph = virtuoso.ListGraphs();



                List<Uri> graphlist = (List<Uri>)graph;
                int j = 0;
                foreach (Uri i in graphlist)
                {
                    j++;
                    if (j < graphlist.Count)
                    {
                        Resultstring.AppendLine("\"" + i.ToString() + "\",");
                    }
                    else
                    {
                        Resultstring.AppendLine("\"" + i.ToString() + "\"");
                    }
                }
                Resultstring.AppendLine("],");
                int t = 0;
                foreach (Uri i in graphlist)
                {
                    try
                    {
                        object types = virtuoso.Query("select * where { <" + i.ToString() + "> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> ?c.}");
                        SparqlResultSet rset = (SparqlResultSet)types;
                        IEnumerable<string> keyenum = rset.Variables;
                        List<string> keylist = keyenum.ToList();
                        string key = "";


                        foreach (string keys in keylist)
                        {
                            key = keys;
                        }

                        Resultstring.Append("\"" + i.ToString() + "\":");
                        int ont = 0;
                        foreach (SparqlResult result1 in rset)
                        {

                            if (result1[key].ToString() == "http://www.w3.org/2002/07/owl#Ontology")
                            {
                                ont++;
                            }
                        }

                        t++;
                        if (t == graphlist.Count)
                        {
                            if (ont > 0)
                            {
                                Resultstring.AppendLine("\"Ontology\"");
                            }
                            else
                            {
                                Resultstring.AppendLine("\"Data\"");
                            }
                        }
                        else
                        {
                            if (ont > 0)
                            {
                                Resultstring.AppendLine("\"Ontology\",");
                            }
                            else
                            {
                                Resultstring.AppendLine("\"Data\",");
                            }
                        }
                    }
                    catch (Exception)
                    {
                        Resultstring.AppendLine("\"" + i.ToString() + "\":\"Data\",");
                    }

                }

                Resultstring.AppendLine("}");

                Result = JObject.Parse(Resultstring.ToString());

            }
            catch (Exception)
            {
                string nullerror = "{\"Result\" : \"error\"}";
                Result = JObject.Parse(nullerror.ToString());

            }

            return Result;
        }

        public class Tree
        {
            public string tree { get; set; }
        }

        //データを消す
        [HttpPost]
        [Route("api/Virtuoso/Delete")]
        public JObject Delete([FromBody] Delete_graph graphObj)
        {
            JObject Result;
            try
            {
                
                VirtuosoManager virtuoso = new VirtuosoManager("localhost", 1111, "DB", "dba", "0443Kou10");
                virtuoso.DeleteGraph(graphObj.graphname);

                string success = "{\"Result\" : \"success\"}";
                Result = JObject.Parse(success.ToString());


            }
            catch (Exception)
            {
                string nullerror = "{\"Result\" : \"error\"}";
                Result = JObject.Parse(nullerror.ToString());

            }

            return Result;
        }

        public class Delete_graph
        {
            public string graphname { get; set; }
        }
    }
}