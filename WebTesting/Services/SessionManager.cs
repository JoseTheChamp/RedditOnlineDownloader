using BakalarBackend;
using System.Linq;
using WebTesting.Entities.Enums;

namespace WebTesting.Services
{
    public class SessionManager
    {
        /*
        Loggedin user:
            AccessToke
            UserName
            RedditId
        






        */

        private class SessionTask {
            public ISession Session { get; set; }
            public SessionResult Result { get; set; }

            public SessionTask(ISession session)
            {
                Session = session;
                Result = SessionResult.Working;
            }
        }
        private List<SessionTask> sessionTasks { get; set; }

        public SessionManager()
        {
            sessionTasks = new List<SessionTask>();
        }

        private void AddSessionTask(ISession session) {
            sessionTasks.Add(new SessionTask(session));
        }
        private void StopSessionTask(ISession session, SessionResult sessionResult) {
            if (sessionResult == SessionResult.Success)
            {
                sessionTasks.Remove((SessionTask)sessionTasks.Where(e => e.Session == session));
            }
            sessionTasks.Where(e => e.Session == session).First().Result = sessionResult;
        }
        public async void LoadSavedPosts(ISession session,Func<string,string,Task<List<Post>>> LoadPosts) {
            AddSessionTask(session);
            string token = string.Empty;
            string userName = string.Empty;
            try { 
                token = session.GetString("AccessToken");
                userName = session.GetString("UserName");
            }catch(Exception ex)
            {
                StopSessionTask(session, SessionResult.UserNotLoggedIn);
                return;
            }
            List<Post> posts = await LoadPosts(token,userName);
            //SAVE LOAD POSTS INTO SESSION
            StopSessionTask(session,SessionResult.Success);
        }
    }
}
