using System;
using System.Collections.Generic;

namespace PlaymentProject
{    
    public class FileSystemCommands
    {
        private Dictionary<int, HashSet<string>> Tree { get; set; } // Adjacency list to represent tree structure of file directory
        private Dictionary<int, string> NodeInfo { get; set; } // Node ID vs name mapping
        private Dictionary<Tuple<int, string>, int> ReverseLookup { get; set; } // Reverse lookup with parent ID and node name combo as key
        private Dictionary<int, int> Parent { get; set; } // Parent ID of each node
        private int CurrentNode { get; set; } // Current node in session state - initially root 
        private int NodeIdCounter { get; set; } // Unique ID for each node
        public FileSystemCommands()
        {
            Init();
        }
        public void Execute(string [] args) // mapping and validation of arguements against commands
        {
            if(args[0] == "ls" && FileSystemUtilities.ValidateArgs(args.Length, 1))
            {                   
                List();
            }                
            else if(args[0] == "pwd" && FileSystemUtilities.ValidateArgs(args.Length, 1))
            {
                PrintWorkingDirectory();
            }
            else if(args[0] == "mkdir" && FileSystemUtilities.ValidateArgs(args.Length, 2))
            {
                MakeDirectory(args[1]);
            }
            else if(args[0] == "cd" && FileSystemUtilities.ValidateArgs(args.Length, 2))
            {
                ChangeDirectory(args[1]);
            }
            else if(args[0] == "rm" && FileSystemUtilities.ValidateArgs(args.Length, 2))
            {
                Remove(args[1]);
            }
            else if(args[0] == "session" && args[1] == "clear" && FileSystemUtilities.ValidateArgs(args.Length, 2))
            {
                Init();
                Console.WriteLine("SUCC: CLEARED: RESET TO ROOT");
            }
            else
            {
                Console.WriteLine("ERR: CANNOT RECOGNIZE INPUT.");
            }
        }
        private void Init() // reset all data
        {
            CurrentNode = 0;
            NodeIdCounter = 0;
            Tree = new Dictionary<int, HashSet<string>>()
            {
                { 0, new HashSet<string>() }
            };
            NodeInfo = new Dictionary<int, string>()
            {
                { 0, "root" }
            };
            Parent = new Dictionary<int, int>() 
            {
                { 0, 0 } // parent of root is itself (terminating condition for upwards traversal)
            };
            ReverseLookup = new Dictionary<Tuple<int, string>, int>()
            {
                {Tuple.Create(0, "root"), 0 }            
            };
        }
        private int Traversal(string path) // validates path and returns last node of traversal
        {
            int traversalNode = string.IsNullOrEmpty(path) || path[0] == '/' ? 0: CurrentNode; // resolving absolute vs relative path
            string [] nodes = path.Split('/', StringSplitOptions.RemoveEmptyEntries); 
            foreach(string node in nodes)
            {
                if(node ==  "..") // reset traversalNode to its parent node
                {
                    traversalNode = Parent[traversalNode];                    
                }
                else if(node != ".") // no need to do anything if node is . (current node)
                {
                    if(!Tree[traversalNode].Contains(node)) // node not present in parent's adjacency list
                    {
                        Console.WriteLine("ERR: INVALID PATH");
                        return -1; // -1 return value indicates invalid path
                    }
                    traversalNode = ReverseLookup[Tuple.Create(traversalNode, node)];
                }                
            }
            return traversalNode;
        }
        private int CreateNode(int parentNodeId, string nodeName)
        {
            /*
            Creating new node - 
                Assigning Id
                Establishing parent child relation
                Updating ReverseLookup
            */                    
            NodeIdCounter++;
            Tree[parentNodeId].Add(nodeName);
            Tree[NodeIdCounter] = new HashSet<string>();
            NodeInfo[NodeIdCounter] = nodeName;
            Parent[NodeIdCounter] = parentNodeId;
            ReverseLookup[Tuple.Create(parentNodeId, nodeName)] = NodeIdCounter;
            return NodeIdCounter;
        }
        private int RemoveNode(int nodeId, string nodeName)
        {
            /*
            Removing node - 
                Breaking parent child relation
                Updating ReverseLookup
            */
            int parentNode = Parent[nodeId];
            Tree[parentNode].Remove(nodeName);
            ReverseLookup.Remove(Tuple.Create(parentNode, nodeName));
            Parent[nodeId] = 0;
            return nodeId;
        }
        private void List()
        {
            Console.Write("DIRS: ");
            foreach(string node in Tree[CurrentNode]) // list children of current node
            {
                Console.Write(string.Format("{0}\t", node));
            }
            Console.WriteLine();
        }
        private void PrintWorkingDirectory()
        {
            Stack<string> path = new Stack<string>();
            int traversalNode = CurrentNode;
            while(Parent[traversalNode] != traversalNode) // upward traversal stops when parent of node is itself (root)
            {
                path.Push(NodeInfo[traversalNode]);
                traversalNode = Parent[traversalNode]; // move one level up
            }
            Console.Write("PATH: /");
            while(path.Count > 0)
            {
                Console.Write(string.Format("{0}/", path.Pop())); // print popped elements of stack to print working directory's path in correct order            
            }
            Console.WriteLine();
        }
        private int MakeDirectory(string path)
        {
            if(path == "/") // cannot create root
            {
                Console.WriteLine("ERR: CANNOT CREATE ROOT");
                return -1;
            }
            path = FileSystemUtilities.RemoveLastSlash(path);
            string nodeToCreate;
            int lastIndexOfSlash = path.LastIndexOf('/'), lastNode = -1;
            if(lastIndexOfSlash == -1) // no slash present in path, input is the node to create 
            {
                nodeToCreate = path;
                if(nodeToCreate == ".." || nodeToCreate == ".")
                {
                    Console.WriteLine("ERR: NODE TO CREATE SHOULD BE EXPANDED NAME");
                    return -1; // return as soon as breaking condition is identified
                }
                lastNode = CurrentNode;
            }
            else
            {
                nodeToCreate = path.Substring(lastIndexOfSlash + 1); // part of path after last /
                if(nodeToCreate == ".." || nodeToCreate == ".")
                {
                    Console.WriteLine("ERR: NODE TO CREATE SHOULD BE EXPANDED NAME");
                    return -1;
                }
                lastNode = Traversal(path.Substring(0, lastIndexOfSlash)); // pass path's substring as arguement, because nodeToCreate is not present in current file tree
            }
            if(lastNode != -1)
            {
                if(Tree[lastNode].Contains(nodeToCreate)) // node already present in parent's adjacency list
                {
                    Console.WriteLine("ERR: DIRECTORY ALREADY EXISTS");                        
                }
                lastNode = CreateNode(lastNode, nodeToCreate);
                Console.WriteLine("SUCC: CREATED");
            }
            return lastNode;          
        }
        private int ChangeDirectory(string path)
        {
            int lastNode = Traversal(path);
            if(lastNode != -1)
            {
                CurrentNode = lastNode; // updating session's current node location
                Console.WriteLine("SUCC: REACHED");
            }
            return CurrentNode;
        }
        private int Remove(string path)
        {
            if(path == "/") // cannot remove root
            {
                Console.WriteLine("ERR: CANNOT REMOVE ROOT");
                return -1;
            }
            path = FileSystemUtilities.RemoveLastSlash(path);
            int lastIndexOfSlash = path.LastIndexOf('/');
            string nodeToRemove = lastIndexOfSlash == -1 ? path : path.Substring(lastIndexOfSlash + 1); // if no slash present in path, input is the node to create, else extract it as substring
            if(nodeToRemove == ".." || nodeToRemove == ".")
            {
                Console.WriteLine("ERR: NODE TO CREATE SHOULD BE EXPANDED NAME");
                return -1;
            }
            int lastNode = Traversal(path);
            if(lastNode != -1)
            {
                if(!Tree[Parent[lastNode]].Contains(nodeToRemove)) // node not present in directory
                {
                    Console.WriteLine("ERR: INVALID PATH");
                    return -1;
                }
                RemoveNode(lastNode, nodeToRemove);
                Console.WriteLine("SUCC: DELETED");
            }
            return lastNode;         
        }
    }
}