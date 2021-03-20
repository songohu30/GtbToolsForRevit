using Autodesk.Revit.DB;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViewModels;

namespace DurchbruchRotationFix
{
    public class DurchbruchRotatedModel
    {
        public FamilyInstance FamilyInstance;
        DurchbruchOrientation _durchbruchOrientation;
        XYZ _basisX;
        XYZ _basisZ;
        public bool IsRotated { get; private set; }
        public bool IsWallHosted { get; private set; }
        public DurchbruchShape DurchbruchShape { get; private set; }
        public bool IsSymbolVisible { get; private set; }
        public DurchbruchRotation DurchbruchRotation { get; private set; }

        private DurchbruchRotatedModel()
        {

        }

        public static DurchbruchRotatedModel Initialize(FamilyInstance familyInstance)
        {
            DurchbruchRotatedModel result = new DurchbruchRotatedModel();
            result.FamilyInstance = familyInstance;
            result.SetTransformations();
            result.SetDurchBruchOrientation();
            result.CheckRotation();
            result.CheckHost();
            result.CheckSymbolVisibility();
            result.CheckDurchbruchShape();
            result.AnalyzeRotation();
            return result;
        }

        private void AnalyzeRotation()
        {
            XYZ vectorY = new XYZ(0, 1, 0);
            double rotationY = _basisX.AngleTo(vectorY) * 180 / Math.PI;
            if (_durchbruchOrientation == DurchbruchOrientation.WallLeft)
            {
                DurchbruchRotation = DurchbruchRotation.Rotated;
                if (Math.Abs(180-rotationY) < 0.1 || Math.Abs(180 - rotationY) == 0.1) DurchbruchRotation = DurchbruchRotation.NotRotated;
                if (Math.Abs(rotationY) < 0.1 || Math.Abs(rotationY) == 0.1) DurchbruchRotation = DurchbruchRotation.Rotated180;
                if (Math.Abs(90 - rotationY) < 0.1 || Math.Abs(90 - rotationY) == 0.1) DurchbruchRotation = DurchbruchRotation.Rotated90;
            }
            if (_durchbruchOrientation == DurchbruchOrientation.WallRight)
            {
                DurchbruchRotation = DurchbruchRotation.Rotated;
                if (Math.Abs(180 - rotationY) < 0.1 || Math.Abs(180 - rotationY) == 0.1) DurchbruchRotation = DurchbruchRotation.Rotated180;
                if (Math.Abs(rotationY) < 0.1 || Math.Abs(rotationY) == 0.1) DurchbruchRotation = DurchbruchRotation.NotRotated;
                if (Math.Abs(90 - rotationY) < 0.1 || Math.Abs(90 - rotationY) == 0.1) DurchbruchRotation = DurchbruchRotation.Rotated90;
            }
            XYZ vectorX = new XYZ(1, 0, 0);
            double rotationX = _basisX.AngleTo(vectorX) * 180 / Math.PI;
            if(_durchbruchOrientation == DurchbruchOrientation.WallUp)
            {
                DurchbruchRotation = DurchbruchRotation.Rotated;
                if (Math.Abs(180 - rotationX) < 0.1 || Math.Abs(180 - rotationX) == 0.1) DurchbruchRotation = DurchbruchRotation.NotRotated;
                if (Math.Abs(rotationX) < 0.1 || Math.Abs(rotationX) == 0.1) DurchbruchRotation = DurchbruchRotation.Rotated180;
                if (Math.Abs(90 - rotationX) < 0.1 || Math.Abs(90 - rotationX) == 0.1) DurchbruchRotation = DurchbruchRotation.Rotated90;
            }
            if (_durchbruchOrientation == DurchbruchOrientation.WallDown)
            {
                DurchbruchRotation = DurchbruchRotation.Rotated;
                if (Math.Abs(180 - rotationX) < 0.1 || Math.Abs(180 - rotationX) == 0.1) DurchbruchRotation = DurchbruchRotation.Rotated180;
                if (Math.Abs(rotationX) < 0.1 || Math.Abs(rotationX) == 0.1) DurchbruchRotation = DurchbruchRotation.NotRotated;
                if (Math.Abs(90 - rotationX) < 0.1 || Math.Abs(90 - rotationX) == 0.1) DurchbruchRotation = DurchbruchRotation.Rotated90;
            }
        }

        private void CheckDurchbruchShape()
        {
            Parameter diameter = FamilyInstance.get_Parameter(new Guid("9c805bcc-ebc9-4d4c-8d73-26970789417a"));
            if(diameter != null)
            {
                DurchbruchShape = DurchbruchShape.Round;
            }
            else
            {
                DurchbruchShape = DurchbruchShape.Rectangular;
            }
        }

        private void CheckRotation()
        {
            double checkedValue = Math.Abs(_basisX.Z);
            if(checkedValue < 0.00174532836590042)
            {
                IsRotated = false;
            }
            else
            {
                IsRotated = true;
            }
        }

        private void CheckHost()
        {
            if(_durchbruchOrientation == DurchbruchOrientation.WallLeft || _durchbruchOrientation == DurchbruchOrientation.WallRight || _durchbruchOrientation == DurchbruchOrientation.WallUp || _durchbruchOrientation == DurchbruchOrientation.WallDown)
            {
                IsWallHosted = true;
            }
            else
            {
                IsWallHosted = false;
            }
        }

        private void CheckSymbolVisibility()
        {
            double checkedValue1 = Math.Abs(_basisX.Z);
            double checkedValue2 = Math.Abs(_basisX.Y);
            if (checkedValue1 < 0.00174532836590042 || checkedValue2 < 0.00174532836590042)
            {
                IsSymbolVisible = true;
            }
            else
            {
                IsSymbolVisible = false;
            }
        }

        public bool FixDurchbruchRotation(Document document)
        {
            try
            {
                //methods for wall openings
                if (_durchbruchOrientation == DurchbruchOrientation.WallLeft) FixRotationWallLeft(document);
                if (_durchbruchOrientation == DurchbruchOrientation.WallRight) FixRotationWallRight(document);
                if (_durchbruchOrientation == DurchbruchOrientation.WallUp) FixRotationWallUp(document);
                if (_durchbruchOrientation == DurchbruchOrientation.WallDown) FixRotationWallDown(document);
                //methods for floor and ceiling openings
                //two options here
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void SetTransformations()
        {
            _basisX = FamilyInstance.GetTransform().BasisX;
            _basisZ = FamilyInstance.GetTransform().BasisZ;
        }

        private void SetDurchBruchOrientation()
        {
            if (_basisZ.X == -1) _durchbruchOrientation = DurchbruchOrientation.WallLeft;
            if (_basisZ.X == 1) _durchbruchOrientation = DurchbruchOrientation.WallRight;
            if (_basisZ.Y == -1) _durchbruchOrientation = DurchbruchOrientation.WallDown;
            if (_basisZ.Y == 1) _durchbruchOrientation = DurchbruchOrientation.WallUp;
            if (_basisZ.Z== 1) _durchbruchOrientation = DurchbruchOrientation.FloorUp;
            if (_basisZ.Z == -1) _durchbruchOrientation = DurchbruchOrientation.FloorDown;
        }

        private void FixRotationWallLeft(Document doc)
        {
            double desiredAngle = Math.PI; //180
            XYZ referenceVector = new XYZ(0, 1, 0);
            double currentAngle = _basisX.AngleTo(referenceVector); //radians
            double rotationAngle;
            Line rotationAxis = GetRotationAxis(1, 0, 0);
            if(_basisX.Z < 0)
            {
                rotationAngle = currentAngle - desiredAngle;
            }
            else
            {
                rotationAngle = desiredAngle - currentAngle;
            }
            ElementTransformUtils.RotateElement(doc, FamilyInstance.Id, rotationAxis, rotationAngle);
        }

        private void FixRotationWallRight(Document doc)
        {
            XYZ referenceVector = new XYZ(0, 1, 0);
            double currentAngle = _basisX.AngleTo(referenceVector); //radians
            double rotationAngle;
            Line rotationAxis = GetRotationAxis(1, 0, 0);
            if (_basisX.Z < 0)
            {
                rotationAngle = currentAngle;
            }
            else
            {
                rotationAngle = -currentAngle;
            }
            ElementTransformUtils.RotateElement(doc, FamilyInstance.Id, rotationAxis, rotationAngle);
        }

        private void FixRotationWallUp(Document doc)
        {
            double desiredAngle = Math.PI; //180
            XYZ referenceVector = new XYZ(1, 0, 0);
            double currentAngle = _basisX.AngleTo(referenceVector); //radians
            double rotationAngle;
            Line rotationAxis = GetRotationAxis(0, 1, 0);
            if (_basisX.Z < 0)
            {
                rotationAngle = desiredAngle -currentAngle;
            }
            else
            {
                rotationAngle = currentAngle - desiredAngle;
            }
            ElementTransformUtils.RotateElement(doc, FamilyInstance.Id, rotationAxis, rotationAngle);
        }

        private void FixRotationWallDown(Document doc)
        {
            XYZ referenceVector = new XYZ(1, 0, 0);
            double currentAngle = _basisX.AngleTo(referenceVector); //radians
            double rotationAngle;
            Line rotationAxis = GetRotationAxis(0, 1, 0);
            if (_basisX.Z < 0)
            {
                rotationAngle = -currentAngle;
            }
            else
            {
                rotationAngle = currentAngle;
            }
            ElementTransformUtils.RotateElement(doc, FamilyInstance.Id, rotationAxis, rotationAngle);
        }

        private Line GetRotationAxis(double dX, double dY, double dZ)
        {
            LocationPoint locPoint = FamilyInstance.Location as LocationPoint;
            XYZ point1 = locPoint.Point;
            XYZ point2 = new XYZ(point1.X + dX, point1.Y + dY, point1.Z + dZ);
            return Line.CreateBound(point1, point2);
        }

    }
}
