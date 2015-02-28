using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArmDuinoBase.Model
{
    public class KinectGestureProcessor:INotifyPropertyChanged
    {

        private bool leftGrip;
        public bool LeftGrip
        {
            get
            {
                return leftGrip;
            }
            set
            {
                leftGrip = value;
                NotifyPropertyChanged("LeftGrip");
            }
        }
        private bool rightGrip;
        public bool RightGrip
        {
            get
            {
                return rightGrip;
            }
            set
            {
                rightGrip = value;
                NotifyPropertyChanged("RightGrip");
            }
        }

        private bool leftSeparated;
        public bool LeftSeparated
        {
            get
            {
                return leftSeparated;
            }
            set
            {
                leftSeparated = value;
                NotifyPropertyChanged("LeftSeparated");
            }
        }
        private bool rightSeparated;
        public bool RightSeparated
        {
            get
            {
                return rightSeparated;
            }
            set
            {
                rightSeparated = value;
                NotifyPropertyChanged("RightSeparated");
            }
        }

        private bool leftUp;
        public bool LeftUp
        {
            get
            {
                return leftUp;
            }
            set
            {
                leftUp = value;
                NotifyPropertyChanged("LeftUp");
            }
        }
        private bool rightUp;
        public bool RightUp
        {
            get
            {
                return rightUp;
            }
            set
            {
                rightUp = value;
                NotifyPropertyChanged("RightUp");
            }
        }

        private int ellipseSize;
        public int EllipseSize
        {
            get
            {
                return ellipseSize;
            }
            set
            {
                ellipseSize = value;
                NotifyPropertyChanged("EllipseSize");
            }
        }

        private SkeletonPoint headPosition;
        public SkeletonPoint HeadPosition
        {
            get
            {
                return headPosition;
            }
            set
            {
                headPosition = value;
                NotifyPropertyChanged("HeadPosition");
            }
        }

        private SkeletonPoint rightHandPosition;
        public SkeletonPoint RightHandPosition
        {
            get
            {
                return rightHandPosition;
            }
            set
            {
                rightHandPosition = value;
                NotifyPropertyChanged("RightHandPosition");
            }
        }

        private SkeletonPoint leftHandPosition;
        public SkeletonPoint LeftHandPosition
        {
            get
            {
                return leftHandPosition;
            }
            set
            {
                leftHandPosition = value;
                NotifyPropertyChanged("LeftHandPosition");
            }
        }

        private SkeletonPoint leftElbowPosition;
        public SkeletonPoint LeftElbowPosition
        {
            get
            {
                return leftElbowPosition;
            }
            set
            {
                leftElbowPosition = value;
                NotifyPropertyChanged("LeftElbowPosition");
            }
        }

        private SkeletonPoint rightElbowPosition;
        public SkeletonPoint RightElbowPosition
        {
            get
            {
                return rightElbowPosition;
            }
            set
            {
                rightElbowPosition = value;
                NotifyPropertyChanged("RightElbowPosition");
            }
        }

        private SkeletonPoint leftShoulderPosition;
        public SkeletonPoint LeftShoulderPosition
        {
            get
            {
                return leftShoulderPosition;
            }
            set
            {
                leftShoulderPosition = value;
                NotifyPropertyChanged("LeftShoulderPosition");
            }
        }

        private SkeletonPoint rightShoulderPosition;
        public SkeletonPoint RightShoulderPosition
        {
            get
            {
                return rightShoulderPosition;
            }
            set
            {
                rightShoulderPosition = value;
                NotifyPropertyChanged("RightShoulderPosition");
            }
        }


        public KinectGestureProcessor()
        {

        }

        public double RightJointsAngle(Joint leftRef, Joint rightRef, Joint joint, Skeleton skeleton)
        {
            double leftRefX = (double)(skeleton.Joints[leftRef.JointType].Position.X);
            double rightRefX = (double)(skeleton.Joints[rightRef.JointType].Position.X);
            double leftRefY = (double)(skeleton.Joints[leftRef.JointType].Position.Y);
            double rightRefY = (double)(skeleton.Joints[rightRef.JointType].Position.Y);
            double refDistance = Math.Sqrt(Math.Pow(rightRefX - leftRefX, 2) + Math.Pow(rightRefY - leftRefY, 2));
            double jointX = (double)(skeleton.Joints[joint.JointType].Position.X);
            double jointY = (double)(skeleton.Joints[joint.JointType].Position.Y);
            double leftRefToJoint = Math.Sqrt(Math.Pow(jointX - leftRefX, 2) + Math.Pow(jointY - leftRefY, 2));
            double rightRefToJoint = Math.Sqrt(Math.Pow(jointX - rightRefX, 2) + Math.Pow(jointY - rightRefY, 2));
            double argAcos = ((Math.Pow(refDistance, 2) + Math.Pow(rightRefToJoint, 2) - Math.Pow(leftRefToJoint, 2)) / (2 * refDistance * rightRefToJoint));
            if (argAcos > 1) argAcos = 1;
            if (argAcos < -1) argAcos = -1;
            double angle = Math.Acos(argAcos);
            angle = (angle * 360) / (2 * Math.PI);
            angle -= 90;
            if (jointY > rightRefY)
            {
                angle = 180 - angle;
            }
            if (angle < 0) angle = -angle;
            if (angle > 180) angle = 180;
            return angle;
        }

        public int GetVerticalAngle(Skeleton skeleton)
        {
            float shoulderZ = skeleton.Joints[JointType.ShoulderLeft].Position.Z;
            float handZ = skeleton.Joints[JointType.HandLeft].Position.Z;
            float angle = (float)Math.Sqrt(Math.Abs(shoulderZ * shoulderZ - handZ * handZ));
            angle *= 100;
            if (angle > 180f) return 180;
            if (angle < 0f) return 0;
            angle = (angle * 180f) / 130f;
            return (int)angle;
        }

        public double LeftJointsAngle(Joint leftRef, Joint rightRef, Joint joint, Skeleton skeleton)
        {
            double leftRefX = (double)(skeleton.Joints[leftRef.JointType].Position.X);
            double rightRefX = (double)(skeleton.Joints[rightRef.JointType].Position.X);
            double leftRefY = (double)(skeleton.Joints[leftRef.JointType].Position.Y);
            double rightRefY = (double)(skeleton.Joints[rightRef.JointType].Position.Y);
            double refDistance = Math.Sqrt(Math.Pow(rightRefX - leftRefX, 2) + Math.Pow(rightRefY - leftRefY, 2));
            double jointX = (double)(skeleton.Joints[joint.JointType].Position.X);
            double jointY = (double)(skeleton.Joints[joint.JointType].Position.Y);
            double leftRefToJoint = Math.Sqrt(Math.Pow(jointX - leftRefX, 2) + Math.Pow(jointY - leftRefY, 2));
            double rightRefToJoint = Math.Sqrt(Math.Pow(jointX - rightRefX, 2) + Math.Pow(jointY - rightRefY, 2));
            double argAcos = ((Math.Pow(leftRefToJoint, 2) + Math.Pow(refDistance, 2) - Math.Pow(rightRefToJoint, 2)) / (2f * leftRefToJoint * refDistance));
            if (argAcos > 1) argAcos = 1;
            if (argAcos < -1) argAcos = -1;
            double angle = Math.Acos(argAcos);
            angle = (angle * 360f) / (2f * Math.PI);
            angle -= 90;
            if (jointY > rightRefY)
            {
                angle = 180 - angle;
            }
            if (angle < 0) angle = -angle;
            if (angle > 180) angle = 180;
            return angle;
        }


        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string property)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(property));
        }
    }
}
