﻿using AutomationSystem.Halcon;
using HalconDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomationSystem.XLDMatch
{
    [Serializable]
    public class XLDMatch : IImageHalconObject
    {
        public HShapeModel hShapeModel;
        public double AngleStart = 0;
        public double AngleEnd = 0;
        public double ScaleMin = 0.9;
        public double ScaleMax = 1.1;
        public double MinScore = 0.5;
        public int MatchCount = 1;
        public double Overlap = 0.5;
        public int Pyramid = 5;
        public double Greediness = 0.5;
        public string ModelImage = "";
        public double ModelCenterX = 0;
        public double ModelCenterY = 0;

        public XLDMatch()
        {
            GetDataManager.AddOutputInt("匹配数量");
            GetDataManager.AddOutputDoubleVector("位置X");
            GetDataManager.AddOutputDoubleVector("位置Y");
            GetDataManager.AddOutputDoubleVector("角度");
            GetDataManager.AddOutputDoubleVector("缩放比");
            GetDataManager.AddOutputDoubleVector("匹配度");
            GetDataManager.AddOutputDouble("轮廓中心X");
            GetDataManager.AddOutputDouble("轮廓中心Y");
        }

        public override void EditParameters()
        {
            XLDMatchForm shapeMatchForm = new XLDMatchForm(this);
            shapeMatchForm.ShowDialog();
            if (shapeMatchForm.Result == System.Windows.Forms.DialogResult.OK)
            {
                hShapeModel = shapeMatchForm.hShapeModel.Clone();
                AngleStart = shapeMatchForm.AngleStart;
                AngleEnd = shapeMatchForm.AngleEnd;
                ScaleMin = shapeMatchForm.ScaleMin;
                ScaleMax = shapeMatchForm.ScaleMax;
                MinScore = shapeMatchForm.MinScore;
                MatchCount = shapeMatchForm.MatchCount;
                Overlap = shapeMatchForm.Overlap;
                Pyramid = shapeMatchForm.Pyramid;
                Greediness = shapeMatchForm.Greediness;
                ModelImage = shapeMatchForm.ModelImage;
                m_listROI = shapeMatchForm.m_listROI;
                ModelCenterX = shapeMatchForm.ModelCenterX;
                ModelCenterY = shapeMatchForm.ModelCenterY;
                IsSetupOK = true;
            }
            else
            {
                IsSetupOK = false;
            }
            if (shapeMatchForm.hShapeModel != null)
            {
                shapeMatchForm.hShapeModel.Dispose();
            }
        }

        public override void Execute(ref HImage source, ref List<ShowObject> showObjects, ref List<ShowText> showTexts)
        {
            double angleStart = 3.14 * AngleStart / 180.0;
            double angleExtent = 3.14 * AngleEnd / 180.0 - angleStart;
            HTuple row, column, angle, scale, score;
            if (source == null || source.Key == IntPtr.Zero)
            {
                throw new RunException(1);
            }
            source.FindScaledShapeModel(hShapeModel, angleStart, angleExtent, ScaleMin, ScaleMax, MinScore, MatchCount, Overlap, "least_squares", Pyramid, Greediness, out row, out column, out angle, out scale, out score);
            GetDataManager.SetOutputInt("匹配数量", row.Length);
            GetDataManager.SetOutputDoubleVector("位置X", column.DArr.ToList());
            GetDataManager.SetOutputDoubleVector("位置Y", row.DArr.ToList());
            GetDataManager.SetOutputDoubleVector("角度", angle.DArr.ToList());
            GetDataManager.SetOutputDoubleVector("缩放比", scale.DArr.ToList());
            GetDataManager.SetOutputDoubleVector("匹配度", score.DArr.ToList());
            GetDataManager.SetOutputDouble("轮廓中心X", ModelCenterX);
            GetDataManager.SetOutputDouble("轮廓中心Y", ModelCenterY);
            if (score.Length == 0)
            {
                throw new RunException(2);
            }
            HXLDCont hXLDCont = hShapeModel.GetShapeModelContours(1);
            for (int i = 0; i < score.Length; i++)
            {
                //创建二维矩阵
                HHomMat2D hv_HomMat2D = new HHomMat2D();
                hv_HomMat2D.HomMat2dIdentity();
                //识别到的角度
                HHomMat2D transMat;
                transMat = hv_HomMat2D.HomMat2dRotate(angle.TupleSelect(i), 0, 0);
                //识别到的行列坐标
                transMat = transMat.HomMat2dTranslate(row.TupleSelect(i), column.TupleSelect(i));
                //变换
                HXLDCont findCont;
                findCont = hXLDCont.AffineTransContourXld(transMat);
                showObjects.Add(new AutomationSystem.Halcon.ShowObject(findCont.Clone(), "green"));
                findCont.Dispose();
            }
            hXLDCont.Dispose();
        }

        public override void SetParameters()
        {
            XLDMatchForm shapeMatchForm = new XLDMatchForm();
            shapeMatchForm.ShowDialog();
            if (shapeMatchForm.Result == System.Windows.Forms.DialogResult.OK)
            {
                hShapeModel = shapeMatchForm.hShapeModel.Clone();
                AngleStart = shapeMatchForm.AngleStart;
                AngleEnd = shapeMatchForm.AngleEnd;
                ScaleMin = shapeMatchForm.ScaleMin;
                ScaleMax = shapeMatchForm.ScaleMax;
                MinScore = shapeMatchForm.MinScore;
                MatchCount = shapeMatchForm.MatchCount;
                Overlap = shapeMatchForm.Overlap;
                Pyramid = shapeMatchForm.Pyramid;
                Greediness = shapeMatchForm.Greediness;
                ModelImage = shapeMatchForm.ModelImage;
                m_listROI = shapeMatchForm.m_listROI;
                ModelCenterX = shapeMatchForm.ModelCenterX;
                ModelCenterY = shapeMatchForm.ModelCenterY;
                IsSetupOK = true;
            }
            else
            {
                IsSetupOK = false;
            }
            if (shapeMatchForm.hShapeModel != null)
            {
                shapeMatchForm.hShapeModel.Dispose();
            }
        }

        public override string ToolDescriptText()
        {
            return "XLD匹配";
        }

        public override string ToolName()
        {
            return "XLD匹配";
        }

        public override string ToolType()
        {
            return "定位工具";
        }
    }
}
