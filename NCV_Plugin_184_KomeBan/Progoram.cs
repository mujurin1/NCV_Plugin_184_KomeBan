using Plugin;
using System;
using System.Linq;


namespace NCV_Plugin_184_KomeBan
{
    public class Progoram : IPlugin
    {
        /// <summary>
        /// コメ番が存在しない放送でコメ番に入っている値
        /// </summary>
        public const string NOT_EXIST_NO_STR = "-1";


        /// <summary>
        /// プラグインのバージョン
        /// </summary>
        public string Version => "1.0";

        /// <summary>
        /// プラグインの説明
        /// </summary>
        public string Description => "184の表示名をコメ番にするプラグインです";

        /// <summary>
        /// プラグインのホスト
        /// </summary>
        public IPluginHost Host { get; set; }

        /// <summary>
        /// アプリケーション起動時にプラグインを自動実行するかどうか
        /// </summary>
        public bool IsAutoRun => true;

        /// <summary>
        /// プラグインの名前
        /// </summary>
        public string Name => "184コメ番プラグイン";


        private NCV_Data NCV_Data;

        public void Run()
        {

        }

        public void AutoRun()
        {
            if (NCV_Data != null) return;

            NCV_Data = new NCV_Data();

            // 本来は BroadcastConnected の中で ReceivedComment を設定したいが、
            // ReceivedComment の方が先に来るので色々と工夫している

            Host.BroadcastConnected += Host_BroadcastConnected;
            Host.BroadcastDisConnected += Host_BroadcastDisConnected;

            Host.ReceivedComment += Host_ReceivedComment;
            Host.WaybackCommentReceived += Host_ReceivedComment;
        }

        private void Host_BroadcastConnected(object sender, EventArgs e)
        {
        }

        private void Host_BroadcastDisConnected(object sender, EventArgs e)
        {
            NCV_Data.DisconnectedLive();
        }

        private void Host_ReceivedComment(object sender, ReceivedCommentEventArgs e)
        {
            if (!NCV_Data.ConnectionInited) {
                NCV_Data.ConnectedLive();
            }

            if (e.CommentDataList.Count == 1) {
                var cd = e.CommentDataList[0];
                if (
                    !cd.IsAnonymity ||
                    cd.No is NOT_EXIST_NO_STR ||
                    !string.IsNullOrWhiteSpace(
                        Host.GetUserSettingInPlugin()
                        .UserDataList
                        .FirstOrDefault(ud => ud.UserId == cd.UserId)
                        ?.NickName
                    )
                ) return;

                NCV_Data.UpsertUser(cd.UserId, int.Parse(cd.No));
            } else {
                var userList = Host.GetUserSettingInPlugin()
                    .UserDataList;

                var list = e.CommentDataList
                    .Distinct(LiveCommentDataEqualityComparer.Instance)
                    .Where(cd => cd.IsAnonymity)
                    .Where(cd => !(cd.No is NOT_EXIST_NO_STR))
                    .Where(cd => string.IsNullOrWhiteSpace(
                        userList
                            .FirstOrDefault(x => x.UserId == cd.UserId)
                            ?.NickName)
                    )
                    .ToArray();

                if (list.Length == 0) return;


                foreach (var data in list) {
                    NCV_Data.UpsertUser(data.UserId, int.Parse(data.No));
                }
            }
        }
    }
}