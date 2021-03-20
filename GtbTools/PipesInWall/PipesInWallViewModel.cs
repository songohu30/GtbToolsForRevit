using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipesInWall
{
    public class PipesInWallViewModel
    {
        public List<RevitLinkInstance> Links { get; set; }
        public RevitLinkInstance SelectedLink { get; set; }
        public List<WallFamily> WallFamilies { get; set; }
        public List<ElementId> SelectedWallTypeIds { get; set; }
        public List<Wall> WallInstances { get; set; }
        public List<PipeModel> PipeModels { get; set; }
        public List<PipeViewModel> PipeViewModels { get; set; }
        public ElementId SelectedElement { get; set; }
        public string ExcludeSystemString { get; set; } = "";

        Document _document;
        Document _linkedDocument;
        List<Pipe> _nonVericalPipes;

        private PipesInWallViewModel()
        {

        }

        public static PipesInWallViewModel Initialize(Document document)
        {
            PipesInWallViewModel result = new PipesInWallViewModel();
            result._document = document;
            result.SetLinks();

            return result;
        }

        public void SetPipeDescriptions()
        {
            using(Transaction tx = new Transaction(_document, "Set Pipes Hinweise"))
            {
                tx.Start();
                foreach (PipeModel pm in PipeModels)
                {
                    pm.SetParameters();
                }
                tx.Commit();
            }
        }

        public void SetLinks()
        {
            FilteredElementCollector ficol = new FilteredElementCollector(_document).OfClass(typeof(RevitLinkInstance)).WhereElementIsNotElementType();
            Links = ficol.Select(e => e as RevitLinkInstance).ToList();
        }

        public void SetWallFamilies()
        {
            _linkedDocument = SelectedLink.GetLinkDocument();
            WallFamilies = new List<WallFamily>();
            FilteredElementCollector ficol = new FilteredElementCollector(_linkedDocument).OfClass(typeof(WallType));
            List<WallType> walltypes = ficol.Select(e => e as WallType).ToList();
            Dictionary<string, List<WallType>> wallTypesDictionary = new Dictionary<string, List<WallType>>();
            foreach (WallType wt in walltypes)
            {
                if(wallTypesDictionary.ContainsKey(wt.FamilyName))
                {
                    wallTypesDictionary[wt.FamilyName].Add(wt);
                }
                else
                {
                    List<WallType> list = new List<WallType>() { wt };
                    wallTypesDictionary.Add(wt.FamilyName, list);
                }
            }
            foreach (KeyValuePair<string, List<WallType>> pair in wallTypesDictionary)
            {
                WallFamily wallFamily = new WallFamily() { FamilyName = pair.Key, WallTypes = pair.Value };
                WallFamilies.Add(wallFamily);
            }
        }

        public void GetSelectedWallTypes()
        {
            SelectedWallTypeIds = new List<ElementId>();
            foreach (WallFamily wf in WallFamilies)
            {
                if(wf.IsSelected)
                {
                    foreach (WallType wt in wf.WallTypes)
                    {
                        SelectedWallTypeIds.Add(wt.Id);
                    }
                }
            }
        }

        public void GetAllWallInstances()
        {
            FilteredElementCollector ficol = new FilteredElementCollector(_linkedDocument).OfClass(typeof(Wall));
            WallInstances = ficol.Select(e => e as Wall).Where(e => SelectedWallTypeIds.Contains(e.GetTypeId())).ToList();
        }

        public void AnalyzePipes()
        {
            GetAllNonVerticalPipes();
            PipeModels = new List<PipeModel>();
            foreach (Wall wall in WallInstances)
            {
                foreach (Pipe pipe in _nonVericalPipes)
                {
                    if (OnePointIsInside(wall, pipe))
                    {
                        PipeModel pipeModel = new PipeModel(pipe);
                        pipeModel.SetProperties();
                        pipeModel.PipeStatus = PipeStatus.OnePoint;
                        pipeModel.SetConnectorModels(wall);
                        pipeModel.SetEndResult();
                        PipeModels.Add(pipeModel);
                    }
                    if (BothPointsInsideWall(wall, pipe))
                    {
                        PipeModel pipeModel = new PipeModel(pipe);
                        pipeModel.SetProperties();
                        pipeModel.PipeStatus = PipeStatus.TwoPointsIn;
                        pipeModel.SetConnectorModels(wall);
                        pipeModel.SetEndResult();
                        PipeModels.Add(pipeModel);
                    }
                }
            }
            CreateViewModel();
        }

        private void CreateViewModel()
        {
            PipeViewModels = new List<PipeViewModel>();
            foreach (PipeModel pm in PipeModels)
            {
                PipeViewModel model = new PipeViewModel(pm);
                model.SetViewModel();
                PipeViewModels.Add(model);
            }
        }

        private void GetAllNonVerticalPipes()
        {
            FilteredElementCollector ficol = new FilteredElementCollector(_document).OfClass(typeof(Pipe));
            _nonVericalPipes = ficol.Select(e => e as Pipe).Where(e => !IsPipeVertical(e) && !IsOfBlzSystem(e)).ToList();
        }

        private bool IsPipeVertical(Pipe pipe)
        {
            bool result = false;
            LocationCurve lc = pipe.Location as LocationCurve;
            Line l = lc.Curve as Line;
            XYZ direction = l.Direction;
            double test = Math.Sin(Math.PI / 180 * 89);
            if (Math.Abs(direction.Z) >= test) result = true;
            return result;
        }

        private bool IsOfBlzSystem(Pipe pipe)
        {
            bool result = false;
            Parameter p = pipe.get_Parameter(BuiltInParameter.RBS_PIPING_SYSTEM_TYPE_PARAM);
            string test = p.AsValueString().ToUpper();
            if (test.Contains("BLZ")) result = true;
            return result;
        }

        private bool BothPointsInsideWall(Wall wall, Pipe pipe)
        {
            Element element = wall as Element;
            BoundingBoxXYZ box = element.get_BoundingBox(null);
            XYZ min = box.Min;
            XYZ max = box.Max;

            LocationCurve lc = pipe.Location as LocationCurve;
            Line line = lc.Curve as Line;
            IList<XYZ> points = line.Tessellate();

            XYZ point1 = points[0];
            XYZ point2 = points[1];

            bool x1 = false;
            bool y1 = false;
            bool z1 = false;

            if (point1.X > min.X && point1.X < max.X) x1 = true;
            if (point1.Y > min.Y && point1.Y < max.Y) y1 = true;
            if (point1.Z > min.Z && point1.Z < max.Z) z1 = true;

            bool x2 = false;
            bool y2 = false;
            bool z2 = false;

            if (point2.X > min.X && point2.X < max.X) x2 = true;
            if (point2.Y > min.Y && point2.Y < max.Y) y2 = true;
            if (point2.Z > min.Z && point2.Z < max.Z) z2 = true;

            return x1 && y1 && z1 && x2 && y2 && z2;
        }

        private bool OnePointIsInside(Wall wall, Pipe pipe)
        {
            Element element = wall as Element;
            BoundingBoxXYZ box = element.get_BoundingBox(null);
            XYZ min = box.Min;
            XYZ max = box.Max;

            LocationCurve lc = pipe.Location as LocationCurve;
            Line line = lc.Curve as Line;
            IList<XYZ> points = line.Tessellate();

            ConnectorSet cs = pipe.ConnectorManager.Connectors;
            List<Connector> connectors = new List<Connector>();
            foreach (Connector c in cs)
            {
                connectors.Add(c);
            }

            XYZ point1 = points[0];
            XYZ point2 = points[1];

            bool x1 = false;
            bool y1 = false;
            bool z1 = false;

            if (point1.X > min.X && point1.X < max.X) x1 = true;
            if (point1.Y > min.Y && point1.Y < max.Y) y1 = true;
            if (point1.Z > min.Z && point1.Z < max.Z) z1 = true;

            bool x2 = false;
            bool y2 = false;
            bool z2 = false;

            if (point2.X > min.X && point2.X < max.X) x2 = true;
            if (point2.Y > min.Y && point2.Y < max.Y) y2 = true;
            if (point2.Z > min.Z && point2.Z < max.Z) z2 = true;

            bool one = x1 && y1 && z1;
            bool two = x2 && y2 && z2;

            int check = 0;
            if (one) check++;
            if (two) check++;
            if (check == 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


    }
}
