using NicoLibrary.NicoLiveData;
using Plugin;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NCV_Plugin_184_KomeBan
{
    // 最終的に欲しい主要なオブジェクトは以下
    // * DGV              DataGridView
    //                    不変
    // 
    // * DGV_RowIndexes   DGV に表示している指定ユーザーの ROW インデックスリスト
    //                    放送の接続が変更されない間は不変
    // 
    // * UserDataList     NCVが管理しているユーザー情報
    //                    不変
    // 
    // 
    // SetDGV()             １度のみ ClassType を取得
    //                      NCV実行中不変なNCVのオブジェクトを取得
    // 
    // ConnectedNewLive()   接続している放送が変わらない間 不変なNCVのオブジェクトを取得
    // 
    // DGV_User             このプラグインで扱うユーザーのクラス
    //                      接続している放送が変わらない間 不変なNCVのオブジェクトを取得 (コンストラクタ)


    class NCV_Data
    {
        public DataGridView DGV { get; }
        public Form MainForm { get; }
        public Dictionary<string, DGV_User> DGVUsers { get; private set; }

        public bool ConnectionInited { get; private set; } = false;

        #region DGV のためのプロパティ
        private Assembly mainAssembly;
        /// <summary>
        /// NiconamaCommentViewer.Control_CommentDGV
        /// </summary>
        private Type CommentDGV_TYPE;
        private object CommentDGV;
        #endregion DGV のためのプロパティ

        #region DGV_RowIndexes のためのプロパティ
        private Assembly coreAssembly;
        /// <summary>
        /// NCV_Core.CommentOperation.NiconamaComment
        /// </summary>
        private Type LiveComment_TYPE;
        /// <summary>
        /// NCV_Core.CommentOperation.CommnetUser
        /// </summary>
        private Type CommnetUser_TYPE;
        /// <summary>
        /// NCV_Core.CommentOperation.CommnetUser.CommnetUserData
        /// </summary>
        private object CommnetUsers;
        /// <summary>
        /// int NCV_Core.CommentOperation.CommnetUser.IndexOfUserIdList(string userId);
        /// </summary>
        private MethodInfo IndexOfUserIdList_Method;
        /// <summary>
        /// NCV_Core.CommentOperation.CommnetUser.CommnetUserData
        /// </summary>
        private Type CommnetUserData_TYPE;
        /// <summary>
        /// List &lt; NCV_Core.CommentOperation.CommnetUser+CommnetUserData &gt;
        /// </summary>
        private System.Collections.IList CommnetUserDataList;
        #endregion DGV_RowIndexes のためのプロパティ




        public NCV_Data()
        {
            mainAssembly = Assembly.GetEntryAssembly();
            MainForm = Application.OpenForms[0];

            #region NCV 内部クラス・オブジェクト取得

            #region CommentDGV = Form1.commentContainer.commentDGV
            // DataGridView とその関連情報を管理している
            // CommentDGV = Form1      // instance :  NiconamaCommentViewer.Form1
            //   .commentContainer     // field    :  NiconamaCommentViewer.Control_CommentContainer
            //   .commentDGV           // field    :  NiconamaCommentViewer.Control_CommentDGV
            #endregion CommentDGV = Form1.commentContainer.commentDGV

            #region DGV = CommentDGV.DGV
            // DataGridView (コメントを表示している)
            // DGV = CommentDGV
            //   .DGV                  // field    :  DataGridView
            #endregion DGV = CommentDGV.DGV

            var Form1_TYPE = mainAssembly.GetType("NiconamaCommentViewer.Form1");
            var Control_CommentContainer_TYPE = mainAssembly.GetType("NiconamaCommentViewer.Control_CommentContainer");
            CommentDGV_TYPE = mainAssembly.GetType("NiconamaCommentViewer.Control_CommentDGV");

            var Control_CommentContainer = Form1_TYPE.InvokeMember(
                "commentContainer",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField,
                null,
                MainForm,
                null);

            CommentDGV = Control_CommentContainer_TYPE.InvokeMember(
                "commentDGV",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField,
                null,
                Control_CommentContainer,
                null);
            DGV = (DataGridView)CommentDGV_TYPE.InvokeMember(
                "DGV",
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetField,
                null,
                CommentDGV,
                null);



            // >>> 実際のオブジェクトは ResetDGVUsers() で取得している <<<<
            #region CommnetUsers = CommentDGV.DGV.liveComment.CommnetUsers
            // コメントを管理するクラス (DataGridView に表示しているコメント)
            // LiveComment = CommentDGV
            //   .liveComment           // field    :  NCV_Core.CommentOperation.NiconamaComment
            // 
            // ユーザーを管理するクラス (DataGridView に表示しているユーザー)
            // CommnetUsers = LiveComment
            //   .CommnetUsers          // property :  NCV_Core.CommentOperation.CommnetUser
            #endregion CommnetUsers = CommentDGV.DGV.liveComment.CommnetUsers

            // >>> 実際のオブジェクトは ResetDGVUsers() で取得している <<<<
            #region CommnetUserData = CommnetUsers.CommnetUserDataList[user_idx]
            // 指定ユーザーの CommnetUsers のインデックス
            // CommnetUserIndex = CommnetUsers
            //   .IndexOfUserIdList     // method   :  ユーザーID (生or184) からインデックスを取得
            //    (userId)
            // 指定ユーザーの情報 (DataGridView に表示しているユーザー)
            // CommnetUserData = CommnetUsers
            //   .CommnetUserDataList   // property :  List<>
            //    [CommnetUserIndex]    //          :  NCV_Core.CommentOperation.CommnetUser+CommnetUserData
            #endregion CommnetUserData = CommnetUsers.CommnetUserDataList[user_idx]

            // >>> 実際のオブジェクトは DGV_User() で取得している <<<<
            #region DGV_RowIndexes = CommnetUserData.CommentIndexList
            // 指定ユーザーの Row インデックスリスト (DataGridView のインデックス)
            // DGV_RowIndexes = CommnetUserData
            //   .CommentIndexList      // property :  List<int>
            #endregion DGV_RowIndexes = CommnetUserData.CommentIndexList


            coreAssembly = Assembly.LoadFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "NCV_Core.dll"));
            LiveComment_TYPE = coreAssembly.GetType("NCV_Core.CommentOperation.NiconamaComment");
            CommnetUser_TYPE = coreAssembly.GetType("NCV_Core.CommentOperation.CommnetUser");
            CommnetUserData_TYPE = coreAssembly.GetType("NCV_Core.CommentOperation.CommnetUser+CommnetUserData");

            #endregion NCV 内部クラス・オブジェクト取得


            DGV.Rows.CollectionChanged += Rows_CollectionChanged; ;
        }

        private void Rows_CollectionChanged(object sender, System.ComponentModel.CollectionChangeEventArgs e)
        {
            if (e.Action is System.ComponentModel.CollectionChangeAction.Add) {
                var row = (DataGridViewRow)e.Element;

                var exist = DGVUsers.TryGetValue(row.Cells[2].Value.ToString(), out var ud);
                if (!exist) return;

                row.Cells[2].Value = ud.NoName;
                row.Cells[2].ToolTipText = ud.Id;
            }
        }

        public void ConnectedLive()
        {
            // このプラグインにとって必要な DGV の中にある情報は接続毎にリセットされる
            DGVUsers = new Dictionary<string, DGV_User>();


            var LiveComment = CommentDGV_TYPE.InvokeMember(
                "liveComment",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField,
                null,
                CommentDGV,
                null);
            CommnetUsers = LiveComment_TYPE.InvokeMember(
                "CommnetUsers",
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty,
                null,
                LiveComment,
                null);
            IndexOfUserIdList_Method = CommnetUser_TYPE.GetMethod(
                "IndexOfUserIdList",
                BindingFlags.Public | BindingFlags.Instance);
            CommnetUserDataList = (System.Collections.IList)CommnetUser_TYPE.InvokeMember(
                "CommnetUserDataList",
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty,
                null,
                CommnetUsers,
                null);

            ConnectionInited = true;
        }

        public void DisconnectedLive()
        {
            ConnectionInited = false;
        }

        public void UpsertUser(string id, int no)
        {
            if (!DGVUsers.TryGetValue(id, out var dgvUser)) {
                dgvUser = new DGV_User(this, id);
                DGVUsers.Add(id, dgvUser);
            }
            dgvUser.ChangeNo(no);

        }

        public void RemoveUser(string id)
        {
            DGVUsers.Remove(id);
        }



        /// <summary>
        /// DGV に表示するユーザーリストのユーザーのインデックス
        /// </summary>
        private int DGV_CommnetUserIndexFromUserId(string userId)
        {
            return (int)IndexOfUserIdList_Method.Invoke(CommnetUsers, new object[] { userId });
        }




        public class DGV_User
        {
            public string Id { get; }
            public int CommnetUserIndex { get; private set; }
            public List<int> DGV_RowIndexes { get; private set; }
            public int MinCommentNo { get; private set; } = int.MaxValue;
            public string NoName { get; private set; }

            private readonly NCV_Data NCV_Data;

            /// <summary>
            /// NCV_Core.CommentOperation.CommnetUser.CommnetUserData
            /// </summary>
            private object CommnetUser { get; set; }


            private object lock_object = new object();
            //private Task change_task = Task.CompletedTask;

            public DGV_User(NCV_Data ncv_data, string id)
            {
                NCV_Data = ncv_data;
                Id = id;

                Change_DGV_Name_All = delegate {
                    if (DGV_RowIndexes == null) {
                        CommnetUserIndex = ncv_data.DGV_CommnetUserIndexFromUserId(Id);
                        // 他の放送に接続した時に残ってたりすると -1 になる
                        if (CommnetUserIndex == -1) return;

                        CommnetUser = ncv_data.CommnetUserDataList[CommnetUserIndex];
                        DGV_RowIndexes = (List<int>)ncv_data.CommnetUserData_TYPE.InvokeMember(
                            "CommentIndexList",
                            BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty,
                            null,
                            CommnetUser,
                            null);
                    }

                    foreach (var index in DGV_RowIndexes) {
                        NCV_Data.DGV.Rows[index].Cells[2].Value = NoName;
                        NCV_Data.DGV.Rows[index].Cells[2].ToolTipText = Id;
                    }
                };
            }

            readonly Action Change_DGV_Name_All;

            public void ChangeNo(int no)
            {
                if (no > MinCommentNo) return;

                MinCommentNo = no;
                NoName = $"#{no}";

                // 全件修正
                // この時点ではまだコメビュにコメント行が存在しないので５秒待機する

                Task.Run(async () => {
                    await Task.Delay(5000);

                    if (!string.IsNullOrWhiteSpace(
                        Program.Inctance.Host.GetUserSettingInPlugin()
                            .UserDataList
                            .FirstOrDefault(ud => ud.UserId == Id)
                            .NickName)
                    ) return;


                    lock (lock_object) {
                        if (no != MinCommentNo) return;

                        try {
                            // Formアプリのコントロールへの操作は同じスレッドじゃないと駄目
                            NCV_Data.DGV.Invoke(Change_DGV_Name_All);
                        } catch (Exception e) {
                            MessageBox.Show($"184コメ番プラグインでエラーが発生しました\n{e}");
                        }
                    }
                });
            }
        }
    }
}
