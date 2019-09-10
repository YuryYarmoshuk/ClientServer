using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;

namespace UDPServer
{
    class Server
    {
        private const int PORT = 11000;
        private const int SIZE = 2048;

        static void Main(string[] args)
        {
            int recv;
            byte[] data;

            EndPoint endpoint = new IPEndPoint(IPAddress.Any, PORT);
            Socket newSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            newSocket.Bind(endpoint);
            
            while (true)
            {
                Console.WriteLine("Waiting for a client...");
                data = new byte[SIZE];
                recv = newSocket.ReceiveFrom(data, ref endpoint);

                string message = Encoding.UTF8.GetString(data, 0, recv);

                if (recv == null)
                {
                    break;
                }

                if (message.IndexOf("<Create>") == 0)
                {
                    string usern = UserSeporate(message, "<Username>");
                    string userp = UserSeporate(message, "<Password>");
                    
                    string dataSend = Create(usern, userp);
                    byte[] byteSend = Encoding.UTF8.GetBytes(dataSend);
                    newSocket.SendTo(byteSend, endpoint);
                    Console.WriteLine("\n" + "Create new user " + usern + " for " + endpoint.ToString());
                }

                /*else if (message.IndexOf("<CreateG>") == 0)
                {
                    string usern = UserSeporate(message, "<Username>");
                    string userp = UserSeporate(message, "<Password>");
                    string groupn = UserSeporate(message, "<Groupname>");

                    string dataSend = Create(usern, userp, groupn);
                    byte[] byteSend = Encoding.UTF8.GetBytes(dataSend);
                    newSocket.SendTo(byteSend, endpoint);
                    Console.WriteLine("\n" + "Create new user " + usern + " in group " + groupn + " for " + endpoint.ToString());
                }*/

                else if (message.IndexOf("<SetGroup>") == 0)
                {
                    string usern = UserSeporate(message, "<Username>");
                    string groupn = UserSeporate(message, "<Groupname>");

                    string dataSend = SetInGroup(usern, groupn);
                    byte[] byteSend = Encoding.UTF8.GetBytes(dataSend);
                    newSocket.SendTo(byteSend, endpoint);
                    Console.WriteLine("\n" + "Set user " + usern + " in group " + groupn + " for " + endpoint.ToString());
                }

                else if (message.IndexOf("<OutputG>") == 0)
                {
                    string dataSend = OutputGroup();
                    byte[] byteSend = Encoding.UTF8.GetBytes(dataSend);
                    newSocket.SendTo(byteSend, endpoint);
                    Console.WriteLine("\n" + "Output all groups" + " for " + endpoint.ToString());
                }

                else if (message.IndexOf("<OutputU>") == 0)
                {
                    string dataSend = OutputUser();
                    byte[] byteSend = Encoding.UTF8.GetBytes(dataSend);
                    newSocket.SendTo(byteSend, endpoint);
                    Console.WriteLine("\n" + "Output all users" + " for " + endpoint.ToString());
                }

                else if (message.IndexOf("<Delete>") == 0)
                {
                    string usern = UserSeporate(message, "<Username>");

                    string dataSend = DeleteUser(usern);
                    byte[] byteSend = Encoding.UTF8.GetBytes(dataSend);
                    newSocket.SendTo(byteSend, endpoint);
                    Console.WriteLine("\n" + "Delete user: " + usern + " for " + endpoint.ToString());
                }

                else if (message.IndexOf("<DeleteG>") == 0)
                {
                    string groupn = UserSeporate(message, "<Groupname>");

                    string dataSend = DeleteGroup(groupn);
                    byte[] byteSend = Encoding.UTF8.GetBytes(dataSend);
                    newSocket.SendTo(byteSend, endpoint);
                    Console.WriteLine("\n" + "Delete group: " + groupn + " for " + endpoint.ToString());
                }

                else if (message.IndexOf("<Group>") == 0)
                {
                    string groupn = UserSeporate(message, "<Groupname>");
                    string dataSend = CreateGroup(groupn);

                    byte[] byteSend = Encoding.UTF8.GetBytes(dataSend);
                    newSocket.SendTo(byteSend, endpoint);
                    Console.WriteLine("\n" + "Create group: " + groupn + " for " + endpoint.ToString());
                }
                else
                {
                    byte[] byteSend = Encoding.UTF8.GetBytes("Unknow command");
                    newSocket.SendTo(byteSend, endpoint);
                    Console.WriteLine("\n" + "Unknow command" + " for " + endpoint.ToString());
                }
            }
            Console.Read();
            newSocket.Close();
        }

        private static string Create(string userName, string userPass)
        {
            try
            {
                DirectoryEntry NewUser;
                DirectoryEntry AD = new DirectoryEntry("WinNT://" + Environment.MachineName + ",computer");

                NewUser = AD.Children.Add(userName, "user");
                NewUser.Invoke("SetPassword", new object[] { userPass });
                NewUser.Invoke("Put", new object[] { "Description", "Test User from .NET" });
                try
                {
                    NewUser.CommitChanges();
                }
                catch (Exception)
                {
                    return "User already exist!!";
                }
                
            }
            catch (System.DirectoryServices.DirectoryServicesCOMException E)
            {
                return "Not create!";    
            }
            return "Create!";
        }

        private static string SetInGroup(string userName, string groupName)
        {
            try
            {
                DirectoryEntry NewUser;
                DirectoryEntry AD = new DirectoryEntry("WinNT://" + Environment.MachineName + ",computer");

                DirectoryEntry grp;

                try
                {
                    grp = AD.Children.Find(groupName, "Group");
                }
                catch (Exception)
                {
                    return "Group not found!";
                }

                try
                {
                    NewUser = AD.Children.Find(userName, "user");
                }
                catch (Exception)
                {
                    return "User not found!";
                }

                NewUser = AD.Children.Add(userName, "user");
                try
                {
                    grp.Invoke("Add", new object[] { NewUser.Path.ToString() });
                }
                catch (Exception)
                {
                    return "User already in group!";
                }

                try
                {
                    grp.CommitChanges();
                    return "User: " + userName + " set in " + groupName + " Group";
                }
                catch (Exception)
                {
                    return "User already in group!";
                }

            }
            catch (System.DirectoryServices.DirectoryServicesCOMException E)
            {
                return "Not create!";
            }
            return "Create!";
        }

        /*private static string Create(string userName, string userPass, string groupName)
        {
            try
            {
                DirectoryEntry AD = new DirectoryEntry("WinNT://" + Environment.MachineName + ",computer");

                DirectoryEntry NewUser;
                DirectoryEntry grp;

                try
                {
                    grp = AD.Children.Find(groupName, "Group");
                }
                catch (Exception)
                {
                    return "Group not found!";
                }

               
                NewUser = AD.Children.Add(userName, "user");
                NewUser.Invoke("SetPassword", new object[] { userPass });
                NewUser.Invoke("Put", new object[] { "Description", "Test User from .NET" });

                try
                {
                    NewUser.CommitChanges();
                }
                catch (Exception)
                {
                    return "User already exist!";
                }
                
                if (grp != null)
                {
                    grp.Invoke("Add", new object[] { NewUser.Path.ToString() });
                    grp.CommitChanges();
                }
            }
            catch (System.DirectoryServices.DirectoryServicesCOMException E)
            {
                return "Not create!";
            }
            return "Create!";
        }*/

        private static string UserSeporate(string mess, string submess)
        {
            string word = null;
            int intWord = 0;

            int point = mess.IndexOf(submess);
            
            for (int i = point; mess[i] != '>'; i++)
            {
                intWord = i+2;
            }

            for (int i = intWord; mess[i] != '<'; i++)
            {
                word += mess[i];
            }
            return word;
        }

        private static string OutputGroup()
        {
            string userList = "";

            DirectoryEntry localMachine = new DirectoryEntry("WinNT://" + Environment.MachineName + ",computer");
            /*DirectorySearcher searcher = new DirectorySearcher(localMachine);

            SearchResultCollection result = searcher.FindAll();

            foreach (SearchResult res in result)
            {
                Console.WriteLine(res.ToString());
            }*/

            foreach (DirectoryEntry child in localMachine.Children)
            {
                if (child.SchemaClassName == "Group")
                {
                    userList += "Group: " + child.Name +"\n";
                    //Out(child);
                }
            }

            return userList;
        }

        /*private static void Out(DirectoryEntry AD)
        {
            DirectoryEntry localMachine = new DirectoryEntry(AD.Path);

            foreach (DirectoryEntry child in localMachine.Children)
            {
                if (child.SchemaClassName == "User")
                {
                    Console.WriteLine(child.Name);
                }
            }
        }*/

        private static string OutputUser()
        {
            string userList = "";

            DirectoryEntry localMachine = new DirectoryEntry("WinNT://" + Environment.MachineName + ",computer");

            foreach (DirectoryEntry child in localMachine.Children)
            {
                if (child.SchemaClassName == "User")
                {
                    userList += "User: " + child.Name + "\n";
                }
            }

            return userList;
        }

        private static string DeleteUser(string userName)
        {
            DirectoryEntry user;

            DirectoryEntry localMachine = new DirectoryEntry("WinNT://" + Environment.MachineName + ",computer");

            DirectoryEntries users = localMachine.Children;
            try
            {
                user = users.Find(userName);
            }
            catch (Exception erMess)
            {
                return "User not found!";
            }
            users.Remove(user); 

            return "User Delete!";
        }

        private static string DeleteGroup(string groupName)
        {
            DirectoryEntry grp;

            DirectoryEntry localMachine = new DirectoryEntry("WinNT://" + Environment.MachineName + ",computer");

            try
            {
                grp = localMachine.Children.Find(groupName, "Group");
            }
            catch (Exception erMess)
            {
                return "Group not found!";
            }
            localMachine.Children.Remove(grp);

            return "Group Delete!";
        }

        private static string CreateGroup(string groupName)
        {
            string result = null;
            DirectoryEntry AD = new DirectoryEntry("WinNT://" + Environment.MachineName + ",computer");

            DirectoryEntry newGroup;
            newGroup = AD.Children.Add(groupName, "Group");
            try
            {
                newGroup.CommitChanges();
                result = "Group " + groupName + " create";
            }
            catch(Exception)
            {
                result = "Group already exists";
            }
 
            return result;
        }
    }
}
 