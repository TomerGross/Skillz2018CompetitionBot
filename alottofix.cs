        public static Location currloc = null;
        public static Location lastloc = null;
        
        public void GetInformation(PirateGame game, Pirate pirate)
        {
            if (lastloc == null && currloc == null)
                {
                    lastloc = pirate.GetLocation();
                    currloc = pirate.GetLocation();
                    
                }
            
            lastloc = currloc;
            currloc = pirate.GetLocation();
            
            System.Console.WriteLine("pirates moves from: {0}     to:  {1}", lastloc, currloc);
            
        }
        
        public void GetObjectsOnWay(PirateGame game, Location lastloc, Location currloc)
        {
            List<MapObject> listofmapobjects = new List<MapObject>();
            
            foreach (Pirate pirate in game.GetMyLivingPirates())
                listofmapobjects.Add(pirate);
            foreach (Pirate epirate in game.GetEnemyLivingPirates())
                listofmapobjects.Add(epirate);    
                
            listofmapobjects.Add(game.GetMyCapsule());
            listofmapobjects.Add(game.GetEnemyCapsule());
            listofmapobjects.Add(game.GetMyMothership());
            listofmapobjects.Add(game.GetEnemyMothership());
            
            foreach(MapObject obj in listofmapobjects)
                {
                    int distance = lastloc.Distance(obj);
                     // need to get the point in Distance distance from lastloc
                     // in direct currloc
                    Location locationonroute = lastloc.Towards(currloc, distance);
                    if (obj.GetLocation().Equals(locationonroute))
                        System.Console.WriteLine("sailing to : {0}", obj);
                }
            
        }
        