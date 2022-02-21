/******************************************************************
** 文件名:  SkeletonJointDriver.cs
** 版  权:  (C)
** 创建人:  moshoeu
** 日  期:  2022/02/09
** 描  述:  骨骼关节驱动

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
*******************************************************************/

using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    public class SkeletonJointDriver : MonoBehaviour
    {
        private Animator m_animator;

        /// <summary>
        /// 关节数据
        /// </summary>
        private SkeletonJointData m_jointCtrl;

        /// <summary>
        /// 是否非法人形
        /// </summary>
        private bool m_isInvaildAvatar;

        /// <summary>
        /// 需要驱动的骨骼
        /// </summary>
        public HumanBodyBones[] m_Bones;

        public List<Dictionary<HumanBodyBones, SkeletonJointData.JointInput>> frameInput
            = new List<Dictionary<HumanBodyBones, SkeletonJointData.JointInput>>();

        /// <summary>
        /// 是否开启调试
        /// </summary>
        [SerializeField]
        private bool m_isDebug;

        void Start()
        {
            m_animator = GetComponent<Animator>();

            m_isInvaildAvatar = !(m_animator.avatar?.isHuman ?? false);
            if (m_isInvaildAvatar)
            {
                Debug.LogError($"SkeletonJointDriver.cs: 模型[{gameObject.name}]上的Avatar不是人形，无法进行骨骼驱动！");
                return;
            }

            m_jointCtrl = new SkeletonJointData();
            m_jointCtrl.InitJoints(m_animator, m_Bones);

            //var str = System.IO.File.ReadAllText($"{Application.dataPath}/samplePosTest.txt");
            //var frames = str.Split(';');

            //System.Collections.Generic.Dictionary<int, HumanBodyBones> dict =
            //    new System.Collections.Generic.Dictionary<int, HumanBodyBones>()
            //    {
            //        { 5, HumanBodyBones.LeftUpperArm },
            //        { 6, HumanBodyBones.RightUpperArm },
            //        { 7, HumanBodyBones.LeftLowerArm },
            //        { 8, HumanBodyBones.RightLowerArm },
            //        { 22, HumanBodyBones.LeftHand },
            //        { 23, HumanBodyBones.RightHand },
            //        { 11, HumanBodyBones.LeftUpperLeg },
            //        { 12, HumanBodyBones.RightUpperLeg },
            //        { 13, HumanBodyBones.LeftLowerLeg },
            //        { 14, HumanBodyBones.RightLowerLeg },
            //        { 15, HumanBodyBones.LeftFoot },
            //        { 16, HumanBodyBones.RightFoot },
            //    };

            //foreach (var frame in frames)
            //{
            //    var boneDatas = frame.Split('+');

            //    Dictionary<HumanBodyBones, SkeletonJointController.JointInput> input =
            //        new Dictionary<HumanBodyBones, SkeletonJointController.JointInput>();

            //    foreach (var data in boneDatas)
            //    {
            //        var boneData = data.Split(',');

            //        int boneType = int.Parse(boneData[0]);
            //        float x = float.Parse(boneData[1]);
            //        float y = float.Parse(boneData[2]);
            //        float z = float.Parse(boneData[3]);
            //        Vector3 pos = new Vector3(x, y, z);

            //        if (dict.ContainsKey(boneType))
            //        {
            //            HumanBodyBones bone = dict[boneType];
            //            input.Add(bone, new SkeletonJointController.JointInput()
            //            {
            //                m_BoneType = bone,
            //                m_Pos = pos
            //            });
            //        }
            //    }

            //    input.Add(HumanBodyBones.Hips, new SkeletonJointController.JointInput()
            //    {
            //        m_BoneType = HumanBodyBones.Hips,
            //        m_Pos = (input[HumanBodyBones.LeftUpperLeg].m_Pos + input[HumanBodyBones.RightUpperLeg].m_Pos) / 2f
            //    });
            //    input.Add(HumanBodyBones.Neck, new SkeletonJointController.JointInput()
            //    {
            //        m_BoneType = HumanBodyBones.Neck,
            //        m_Pos = (input[HumanBodyBones.LeftUpperArm].m_Pos + input[HumanBodyBones.RightUpperArm].m_Pos) / 2f
            //    });
            //    input.Add(HumanBodyBones.Spine, new SkeletonJointController.JointInput()
            //    {
            //        m_BoneType = HumanBodyBones.Spine,
            //        m_Pos = (input[HumanBodyBones.LeftUpperArm].m_Pos + input[HumanBodyBones.RightUpperArm].m_Pos + input[HumanBodyBones.Hips].m_Pos) / 3f
            //    });

            //    frameInput.Add(input);




            //}
        }

        int crtIdx = 0;
        float timer = 0;
        void Update()
        {
            if (m_isInvaildAvatar)
            {
                return;
            }

            //timer += Time.deltaTime;
            //if (timer < 1)
            //{
            //    return;
            //}
            //timer = 0;

            if (frameInput.Count == 0) return;

            if (crtIdx >= frameInput.Count)
            {
                crtIdx = 0;
            }

            var frame = frameInput[crtIdx++];
            m_jointCtrl.UpdateJoints(new List<SkeletonJointData.JointInput>(frame.Values).ToArray());
        
            if (m_isDebug)
            {
                DebugJoints();
            }
        }

        private void DebugJoints()
        {
            DrawJoint(m_jointCtrl.m_RootJoint);
            
            void DrawJoint(TreeNode<SkeletonJointData.Joint> jointNode)
            {
                var parentJoint = jointNode.m_Data;
                foreach (var child in jointNode.m_Childs)
                {
                    var childJoint = child.m_Data;
                    LineRenderer lineRender = GetLineRender(parentJoint.m_BoneType, childJoint.m_BoneType);
                    lineRender.positionCount = 2;
                    lineRender.SetPosition(0, parentJoint.m_Pos);
                    lineRender.SetPosition(1, childJoint.m_Pos);

                    DrawJoint(child);
                }
            }

            LineRenderer GetLineRender(HumanBodyBones parent, HumanBodyBones child)
            {
                var root = GameObject.Find("DebugJointRoot");
                if (root == null)
                {
                    root = new GameObject("DebugJointRoot");
                }

                string lineRenderName = $"Joint[{parent}]To[{child}]";
                Transform renderTrans = root.transform.Find(lineRenderName);
                if (renderTrans == null)
                {
                    var obj = new GameObject(lineRenderName);
                    obj.transform.SetParent(root.transform);
                    renderTrans = obj.transform;
                }

                LineRenderer result = renderTrans.GetComponent<LineRenderer>();
                if (result == null)
                {
                    result = renderTrans.gameObject.AddComponent<LineRenderer>();
                    result.startWidth = 0.02f;
                    result.endWidth = 0.02f;
                }
                return result;

            }
        }
    }
}