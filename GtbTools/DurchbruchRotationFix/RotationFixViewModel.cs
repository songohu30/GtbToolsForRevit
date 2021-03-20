using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViewModels;

namespace DurchbruchRotationFix
{
    public class RotationFixViewModel
    {
        public string TotalRound { get; set; }
        public string TotalEckig { get; set; }
        public string NotVisibleRound { get; set; }
        public string NotVisibleEckig { get; set; }
        public string Rotated90Round { get; set; }
        public string Rotated90Eckig { get; set; }
        public string Rotated180Round { get; set; }
        public string Rotated180Eckig { get; set; }
        public List<DurchbruchRotatedModel> DurchbruchRotatedModels { get; set; }
        public List<DurchbruchRotatedModel> NotVisibleRoundList { get; set; }
        public List<DurchbruchRotatedModel> NotVisibleEckigList { get; set; }
        public List<DurchbruchRotatedModel> Rotated90RoundList { get; set; }
        public List<DurchbruchRotatedModel> Rotated90EckigList { get; set; }
        public List<DurchbruchRotatedModel> Rotated180RoundList { get; set; }
        public List<DurchbruchRotatedModel> Rotated180EckigList { get; set; }
        public List<ElementId> Selection { get; set; }
        public List<DurchbruchRotatedModel> RotatedElementsToFix { get; set; }

        private RotationFixViewModel()
        {

        }

        public static RotationFixViewModel Initialize(List<DurchbruchRotatedModel> durchbruchRotatedModels)
        {
            RotationFixViewModel result = new RotationFixViewModel();
            result.DurchbruchRotatedModels = durchbruchRotatedModels;
            result.SetTheLists();
            result.SetTheLabels();
            return result;
        }

        private void SetTheLabels()
        {
            int rund = 0;
            int eckig = 0;
            foreach (var item in DurchbruchRotatedModels)
            {
                if (item.DurchbruchShape == DurchbruchShape.Round) rund++;
                if (item.DurchbruchShape == DurchbruchShape.Rectangular) eckig++;
            }
            TotalRound = rund.ToString();
            TotalEckig = eckig.ToString();
            NotVisibleRound = NotVisibleRoundList.Count.ToString();
            NotVisibleEckig = NotVisibleEckigList.Count.ToString();
            Rotated90Round = Rotated90RoundList.Count.ToString();
            Rotated90Eckig = Rotated90EckigList.Count.ToString();
            Rotated180Round = Rotated180RoundList.Count.ToString();
            Rotated180Eckig = Rotated180EckigList.Count.ToString();
        }

        private void SetTheLists()
        {
            NotVisibleRoundList = new List<DurchbruchRotatedModel>();
            NotVisibleEckigList = new List<DurchbruchRotatedModel>();
            Rotated90RoundList = new List<DurchbruchRotatedModel>();
            Rotated90EckigList = new List<DurchbruchRotatedModel>();
            Rotated180RoundList = new List<DurchbruchRotatedModel>();
            Rotated180EckigList = new List<DurchbruchRotatedModel>();
            foreach (DurchbruchRotatedModel model in DurchbruchRotatedModels)
            {
                if(model.DurchbruchShape == DurchbruchShape.Round)
                {
                    if (model.DurchbruchRotation == DurchbruchRotation.Rotated) NotVisibleRoundList.Add(model);
                    if (model.DurchbruchRotation == DurchbruchRotation.Rotated90) Rotated90RoundList.Add(model);
                    if (model.DurchbruchRotation == DurchbruchRotation.Rotated180) Rotated180RoundList.Add(model);
                }
                if (model.DurchbruchShape == DurchbruchShape.Rectangular)
                {
                    if (model.DurchbruchRotation == DurchbruchRotation.Rotated) NotVisibleEckigList.Add(model);
                    if (model.DurchbruchRotation == DurchbruchRotation.Rotated90) Rotated90EckigList.Add(model);
                    if (model.DurchbruchRotation == DurchbruchRotation.Rotated180) Rotated180EckigList.Add(model);
                }
            }
        }
    }
}
