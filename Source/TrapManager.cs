using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Celeste64
{
    public enum TrapType
    {
        Bald = 0x30,
        Bubble = 0x31,
        Hiccup = 0x32,
        Ice = 0x33,
        Invisible = 0x34,
        Literature = 0x35,
        Reverse = 0x36,
        Stun = 0x37,
        Zoom_In = 0x38,
        Zoom_Out = 0x39
    }

    public enum TrapExpirationAction
    {
        Menu = 0,
        Deaths = 1,
        Screens = 2,
    }

    public class BaseTrapInstance
    {
        public string message;

        public TrapType type;
        public TrapExpirationAction expirationAction;
        public int expirationAmount;

        public DateTime activationTime;

        public bool bIsLinked = false;

        public int trackedDeaths = 0;
        public HashSet<string> trackedScreens = new HashSet<string>();

        public BaseTrapInstance(TrapType type, string message, TrapExpirationAction expirationAction, int expirationAmount, bool bIsLinked = false)
        {
            this.type = type;
            this.message = message;
            this.expirationAction = expirationAction;
            this.expirationAmount = expirationAmount;

            this.activationTime = DateTime.Now;
            this.bIsLinked = bIsLinked;
        }

        public void Activate()
        {
            switch (this.type)
            {
                case TrapType.Bald:
                {
                    break;
                }
                case TrapType.Bubble:
                {
                    Game.Instance.GetWorld()?.Get<Player>()?.BubbleTrap();
                    break;
                }
                case TrapType.Hiccup:
                {
                    this.activationTime = DateTime.Now;
                    this.activationTime = this.activationTime.AddSeconds(3.0f + (Random.Shared.NextDouble() * 12.0f));

                    break;
                }
                case TrapType.Ice:
                {
                    break;
                }
                case TrapType.Invisible:
                {
                    break;
                }
                case TrapType.Literature:
                {
                    string chosenLit = TrapManager.Literature[Random.Shared.Next(TrapManager.Literature.Count())];
                    string[] splitLit = chosenLit.Split('\n');
                    foreach (string litLine in splitLit)
                    {
                        string currentLineText = "";
                        string[] splitLine = litLine.Split(' ');
                        foreach (string litWord in splitLine)
                        {
                            if (currentLineText.Length + litWord.Length + 1 > 70)
                            {
                                ArchipelagoMessage partialLineMessage = new ArchipelagoMessage(currentLineText);
                                partialLineMessage.RemainingTime = 480;
                                Game.Instance.ArchipelagoManager.LiteratureLog.Add(partialLineMessage);
                                Log.Info(currentLineText);
                                currentLineText = "";
                            }
                            currentLineText += litWord + " ";
                        }
                        ArchipelagoMessage finalLineMessage = new ArchipelagoMessage(currentLineText);
                        finalLineMessage.RemainingTime = 480;
                        Log.Info(currentLineText);
                        Game.Instance.ArchipelagoManager.LiteratureLog.Add(finalLineMessage);
                    }

                    this.activationTime = DateTime.Now;

                    break;
                }
                case TrapType.Reverse:
                {
                    break;
                }
                case TrapType.Stun:
                {
                    this.activationTime = DateTime.Now;

                    break;
                }
                case TrapType.Zoom_In:
                {
                    break;
                }
                case TrapType.Zoom_Out:
                {
                    break;
                }
            }
        }

        ~BaseTrapInstance()
        {
            switch (this.type)
            {
                case TrapType.Bald:
                {
                    break;
                }
                case TrapType.Bubble:
                {
                    break;
                }
                case TrapType.Hiccup:
                {
                    break;
                }
                case TrapType.Ice:
                {
                    break;
                }
                case TrapType.Invisible:
                {
                    break;
                }
                case TrapType.Literature:
                {
                    break;
                }
                case TrapType.Reverse:
                {
                    break;
                }
                case TrapType.Stun:
                {
                    break;
                }
                case TrapType.Zoom_In:
                {
                    break;
                }
                case TrapType.Zoom_Out:
                {
                    break;
                }
            }
        }
    }


    public class TrapManager
    {
        public static string[] Literature = [
            "I must not fear. Fear is the mind-killer. Fear is the little-death that brings about total obliteration. I will face my fear. I will permit it to pass over me and through me. And when it has gone past I will turn the inner eye to see its path. Where the fear has gone, there will be nothing. Only I will remain. \n-Herbert",
            "About three things I was absolutely positive.\nFirst, Edward was a vampire.\nSecond, there was a part of him - and I didn't know how potent that part might be - that thirsted for my blood.\nAnd third, I was unconditionally and irrevocably in love with him. \n-Meyer",
            "Say! I like green eggs and ham!\nI do! I like them, Sam-I-am!\nAnd I would eat them in a boat.\nAnd I would eat them with a goat...\nAnd I will eat them in the rain.\nAnd in the dark.\nAnd on a train.\nAnd in a car. And in a tree.\nThey are so good, so good, you see!\nSo I will eat them in a box.\nAnd I will eat them with a fox.\nAnd I will eat them in a house.\nAnd I will eat them with a mouse.\nAnd I will eat them here and there.\nSay! I will eat them anywhere!\nI do so like green eggs and ham!\nThank you! Thank you, Sam-I-am! \n-Seuss",
            "But, soft! what light through yonder window breaks? It is the east, and Juliet is the sun. Arise, fair sun, and kill the envious moon, Who is already sick and pale with grief, That thou her maid art far more fair than she: Be not her maid, since she is envious; Her vestal livery is but sick and green And none but fools do wear it; cast it off. It is my lady, O, it is my love! O, that she knew she were! \n-Shakespeare",
            "It was the best of times, it was the worst of times, it was the age of wisdom, it was the age of foolishness, it was the epoch of belief, it was the epoch of incredulity, it was the season of light, it was the season of darkness, it was the spring of hope, it was the winter of despair. \n-Dickens",
            "Marley was dead, to begin with. There is no doubt whatever, about that. The register of his burial was signed by the clergyman, the clerk, the undertaker, and the chief mourner. Scrooge signed it; and Scrooge's name was good upon 'change, for anything he chose to put his hand to. Old Marley was as dead as a door-nail. \n-Dickens",
            "Many that live deserve death. And some that die deserve life. Can you give it to them? Then do not be too eager to deal out death in judgement. For even the very wise cannot see all ends. \n-Tolkien",
            "It is a truth universally acknowledged, that a single man in possession of a good fortune, must be in want of a wife. \n-Austen",
            "The truth always carries the ambiguity of the words used to express it. \n-Herbert",
            "'I daresay you haven't had much practice,' said the Queen. 'When I was your age, I always did it for half-an-hour a day. Why, sometimes I've believed as many as six impossible things before breakfast.' \n-Carroll",
            "Life, with its rules, its obligations, and its freedoms, is like a sonnet: You're given the form, but you have to write the sonnet yourself. \n-L'Engle",
            "Like the moon over\nthe day my genius and brawn\nare lost on these fools\n-Haiku",
            "In a hole in the ground there lived a hobbit. Not a nasty, dirty, wet hole, filled with the ends of worms and an oozy smell, nor yet a dry, bare, sandy hole with nothing in it to sit down on or to eat: it was a hobbit-hole, and that means comfort. \n-Tolkien",
            "'Good Morning!' said Bilbo, and he meant it. The sun was shining, and the grass was very green. But Gandalf looked at him from under long bushy eyebrows that stuck out further than the brim of his shady hat. 'What do you mean?' he said. 'Do you wish me a good morning, or mean that it is a good morning whether I want it or not, or that you feel good this morning, or that it is a morning to be good on?' 'All of them at once,' said Bilbo. \n-Tolkien",
            "'Good morning!' he said at last. 'We don't want any adventures here, thank you! You might try over The Hill or across The Water.' By this he meant that the conversation was at an end. 'What a lot of things you do use Good morning for!' said Gandalf. 'Now you mean that you want to get rid of me, and that it won't be good till I move off.” \n-Tolkien",
            "'There is nothing like looking, if you want to find something. You certainly usually find something, if you look, but it is not always quite the something you were after.' \n-Tolkien",
            "Drummer, beat, and piper, blow.\nHarper, strike, and soldier, go.\nFree the flame and sear the grasses,\nTil the dawning Red Star passes. \n-McCaffrey",
            "The tears I feel today\nI'll wait to shed tomorrow.\nThough I'll not sleep this night\nNor find surcease from sorrow.\nMy eyes must keep their sight:\nI dare not be tear-blinded.\nI must be free to talk\nNot choked with grief, clear-minded.\nMy mouth cannot betray\nThe anguish that I know.\nYes, I'll keep my tears til later:\nBut my grief will never go. \n-McCaffrey",
            "Who wills,\nCan.\nWho tries,\nDoes.\nWho loves,\nLives. \n-McCaffrey",
            "The little queen all golden\nFlew hissing at the sea.\nTo stop each wave\nHer clutch to save\nShe ventured bravely.\nAs she attacked the sea in rage\nA holderman came nigh\nAlong the sand\nFishnet in hand\nAnd saw the queen midsky.\nHe stared at her in wonder\nFor often he'd been told\nThat such as she\nCould never be\nWho hovered there, bright gold.\nHe saw her plight and quickly\nHe looked up the cliff he faced\nAnd saw a cave\nAbove the wave\nIn which her eggs he placed.\nThe little queen all golden\nUpon his shoulder stood\nHer eyes all blue\nGlowed of her true\nUndying gratitude. \n-McCaffrey",
            "Harper, treat your words with care\nFor they may cause joy or despair\nSing your songs of health and love\nOf dragons flaming from above. \n-McCaffrey",
            "There was only one catch and that was Catch-22, which specified that a concern for one's safety in the face of dangers that were real and immediate was the process of a rational mind. Orr was crazy and could be grounded. All he had to do was ask, and as soon as he did, he would no longer be crazy and would have to fly more missions. Orr would be crazy to fly more missions and sane if he didn't, but if he was sane he had to fly them. If he flew them he was crazy and didn't have to, but if he didn't want to he was sane and had to. \n-Heller",
            "'They're trying to kill me,' Yossarian told him calmly. 'No one's trying to kill you,' Clevinger cried. 'Then why are they shooting at me?' Yossarian asked. 'They're shooting at everyone,' Clevinger answered. 'They're trying to kill everyone.' And what difference does that make? \n-Heller",
            "You have brains in your head. You have feet in your shoes.\nYou can steer yourself any direction you choose.\nYou're on your own. And you know what you know.\nAnd YOU are the one who'll decide where to go... \n-Suess",
            "When you have eliminated all which is impossible, then whatever remains, however improbable, must be the truth. \n-Doyle",
            "'My mind,' he said, 'rebels at stagnation. Give me problems, give me work, give me the most abstruse cryptogram or the most intricate analysis, and I am in my own proper atmosphere. I can dispense then with artificial stimulants. But I abhor the dull routine of existence. I crave for mental exaltation. That is why I have chosen my own particular profession, or rather created it, for I am the only one in the world.' \n-Doyle",
            "Life is infinitely stranger than anything which the mind of man could invent. We would not dare to conceive the things which are really mere commonplaces of existence. If we could fly out of that window hand in hand, hover over this great city, gently remove the roofs, and and peep in at the queer things which are going on, the strange coincidences, the plannings, the cross-purposes, the wonderful chains of events, working through generations, and leading to the most outre results, it would make all fiction with its conventionalities and foreseen conclusions most stale and unprofitable. \n-Doyle",
            "The story so far:\nIn the beginning the Universe was created. This has made a lot of people very angry and been widely regarded as a bad move. \n-Adams",
            "For instance, on the planet Earth, man had always assumed that he was more intelligent than dolphins because he had achieved so much-the wheel, New York, wars and so on-whilst all the dolphins had ever done was muck about in the water having a good time. But conversely, the dolphins had always believed that they were far more intelligent than man—for precisely the same reasons. \n-Adams",
            "It is known that there are an infinite number of worlds, simply because there is an infinite amount of space for them to be in. However, not every one of them is inhabited. Therefore, there must be a finite number of inhabited worlds. Any finite number divided by infinity is as near to nothing as makes no odds, so the average population of all the planets in the Universe can be said to be zero. From this it follows that the population of the whole Universe is also zero, and that any people you may meet from time to time are merely the products of a deranged imagination. \n-Adams",
            "Far over the misty mountains cold\nTo dungeons deep and caverns old\nWe must away,\nere break of day,\nTo seek the pale enchanted gold.\nThe dwarves of yore made mighty spells,\nWhile hammers fell like ringing bells\nIn places deep,\nwhere dark things sleep,\nIn hollow halls beneath the fells.\nFor ancient king and elvish lord\nThere many a gleaming golden hoard\nThey shaped and wrought,\nand light they caught\nTo hide in gems on hilt of sword.\nOn silver necklaces they strung\nThe flowering stars, on crowns they hung\nThe dragon-fire,\nin twisted wire\nThey meshed the light of moon and sun. \n-Tolkien",
            "Far over the misty mountains cold\nTo dungeons deep and caverns old\nWe must away,\nere break of day,\nTo claim our long-forgotten gold.\nGoblets they carved there for themselves And harps of gold, where no man delves\nThere lay they long,\nand many a song\nWas sung unheard by men or elves.\nThe pines were roaring on the height,\nThe winds were moaning in the night.\nThe fire was red,\nit flaming spread.\nThe trees like torches blazed with light. \n-Tolkien",
            "The bells were ringing in the dale\nAnd men looked up with faces pale.\nThen dragon's ire\nmore fierce than fire\nLaid low their towers and houses frail.\nThe mountain smoked beneath the moon.\nThe dwarves, they heard the tramp of doom.\nThey fled their hall,\nto dying fall\nBeneath his feet, beneath the moon.\nFar over the misty mountains grim\nTo dungeons deep and caverns dim\nWe must away,\nere break of day,\nTo win our harps and gold from him! \n-Tolkien",
            "Roads go ever ever on,\nOver rock and under tree,\nBy caves where never sun has shone,\nBy streams that never find the sea.\nOver snow by winter sown,\nAnd through the merry flowers of June,\nOver grass and over stone,\nAnd under mountains of the moon.\nRoads go ever ever on\nUnder cloud and under star,\nYet feet that wandering have gone\nTurn at last to home afar.\nEyes that fire and sword have seen\nAnd horror in the halls of stone\nLook at last on meadows green\nAnd trees and hills they long have known. \n-Tolkien",
            "Three Rings for the Elven-kings under the sky,\nSeven for the Dwarf-lords in their halls of stone,\nNine for Mortal Men doomed to die,\nOne for the Dark Lord on his dark throne\nIn the Land of Mordor where the Shadows lie.\nOne Ring to rule them all, One Ring to find them,\nOne Ring to bring them all, and in the darkness bind them,\nIn the Land of Mordor where the Shadows lie. \n-Tolkien",
            "All that is gold does not glitter,\nNot all those who wander are lost.\nThe old that is strong does not wither,\nDeep roots are not reached by the frost.\nFrom the ashes a fire shall be woken,\nA light from the shadows shall spring.\nRenewed shall be blade that was broken,\nThe crownless again shall be king. \n-Tolkien",
            "Gone away, gone ahead,\nEchoes roll unanswered.\nEmpty, open, dusty, dead,\nWhy have all the Weyrfolk fled?\nWhere have dragons gone together?\nLeaving Weyrs to wind and weather?\nSetting herdbeasts free of tether?\nGone, our safeguards, gone, but whither?\nHave they flown to some new Weyr\nWhen cruel Threads some others fear?\nAre they worlds away from here?\nWhy, oh, why, the empty Weyr? \n-McCaffrey",
            "Love, which quickly arrests the gentle heart,\nSeized him with my beautiful form\nThat was taken from me, in a manner which still grieves me.\nLove, which pardons no beloved from loving,\ntook me so strongly with delight in him\nThat, as you see, it still abandons me not... \n-Alighieri",
            "I cannot express it, but surely you and everybody have a notion that there is or should be an existence of yours beyond you. What were the use of my creation, if I were entirely contained here? My great miseries in this world have been Heathcliff's miseries, and I watched and felt each from the beginning: my great thought in living is himself. If all else perished, and he remained, I should still continue to be, and if all else remained, and he were annihilated, the universe would turn to a mighty stranger: I should not seem a part of it. \n-Bronte",
            "I have dreamt in my life, dreams that have stayed with me ever after, and changed my ideas. They have gone through and through me, like wine through water, and altered the color of my mind. And this is one: I'm going to tell it - but take care not to smile at any part of it. \n-Bronte",
            "This story shall the good man teach his son,\nAnd Crispin Crispian shall ne'er go by,\nFrom this day to the ending of the world,\nBut we in it shall be remembered.\nWe few, we happy few, we band of brothers.\nFor he to-day that sheds his blood with me\nShall be my brother. \n-Shakespeare",
            "However mean your life is, meet it and live it. Do not shun it and call it hard names. It is not so bad as you are. It looks poorest when you are richest. The fault-finder will find faults even in paradise. Love your life, poor as it is. You may perhaps have some pleasant, thrilling, glorious hours, even in a poorhouse. The setting sun is reflected from the windows of the almshouse as brightly as from the rich man's abode; the snow melts before its door as early in the spring. I do not see but a quiet mind may live as contentedly there, and have as cheering thoughts, as in a palace. \n-Thoreau",
            "We must learn to reawaken and keep ourselves awake, not by mechanical aids, but by an infinite expectation of the dawn, which does not forsake us even in our soundest sleep. I know of no more encouraging fact than the unquestionable ability of man to elevate his life by a conscious endeavour. It is something to be able to paint a particular picture, or to carve a statue, and so to make a few objects beautiful, but it is far more glorious to carve and paint the very atmosphere and medium through which we look, which morally we can do. To affect the quality of the day, that is the highest of arts. \n-Thoreau",
            "If one advances confidently in the direction of his dreams, and endeavors to live the life which he has imagined, he will meet with a success unexpected in common hours. He will put some things behind, will pass an invisible boundary. New, universal, and more liberal laws will begin to establish themselves around and within him, or the old laws be expanded, and interpreted in his favor in a more liberal sense, and he will live with the license of a higher order of beings. \n-Thoreau",
            "I'm nobody!\nWho are you?\nAre you nobody, too?\nThen there's a pair of us, don't tell!\nThey'd banish us, you know.\nHow dreary to be somebody!\nHow public, like a frog\nTo tell your name the livelong day\nTo an admiring bog! -Dickinson",
            "How happy is the little stone\nThat rambles in the road alone,\nAnd doesn't care about careers,\nAnd exigencies never fears.\nWhose coat of elemental brown\nA passing universe put on.\nAnd independent as the sun,\nAssociates or glows alone,\nFulfilling absolute decree\nIn casual simplicity. \n-Dickinson",
            "Because I could not stop for death\nHe kindly stopped for me.\nThe carriage held but just ourselves\nAnd immortality \n-Dickinson",
            "For, like almost everyone else in our country, I started out with my share of optimism. I believed in hard work and progress and action, but now, after first being 'for' society and then 'against' it, I assign myself no rank or any limit, and such an attitude is very much against the trend of the times. But my world has become one of infinite possibilities. What a phrase - still it's a good phrase and a good view of life, and a man shouldn't accept any other; that much I've learned underground. Until some gang succeeds in putting the world in a strait jacket, its definition is possibility. \n-Ellison",
            "Tarnished, a word.\n\nWhat if you booted up Resident Evil 4 Remake and Ashley was just a tiny mouse. What would you do?\n\nJust, imagine this: There she is standing there, cute as a button, and with a high-pitched voice she cries out: \"Leon! Help!I can't reach the Gorgonzola! Leon!\".\n\nAnd then after you have helped her up in some sort of quick-time mousecapade, she turns to you, looks you dead in the eyes and says: \"Cheese Whiz, mister! Thanks for the help!\". What would you do?\n\n\nAnyways, find the Albinauric woman.",
            "I'm fully aware of what I'm doing. Can't you see? Man committed a sin... disturbing the life cycle of nature... The original sin that man is responsible to... To protect the life cycle. I have made a creature to rule over mankind... This is the final battle. Show yourself! Our new ruler, the Emperor!",
            "Light thinks it travels faster than anything but it is wrong. No matter how fast light travels, it finds the darkness has always got there first, and is waiting for it. \n-Pratchet",
            "It would be difficult for me to tell you what the moral of this story is. For some stories, it's easy. The moral of 'The Three Bears,' for instance, is 'Never break into someone else's house.' The moral of 'Snow White' is 'Never eat apples.' The moral of World War I is 'Never assassinate Archduke Ferdinand. \n-Snicket",
            "Tell me. For whom do you fight?\n\nHmph! How very glib. And do you believe in Eorzea? Eorzea's unity is forged of falsehoods. Its city-states are built on deceit. And its faith is an instrument of deception.\n\nIt is naught but a cobweb of lies. To believe in Eorzea is to believe in nothing.In Eorzea, the beast tribes often summon gods to fight in their stead--though your comrades only rarely respond in kind. Which is strange, is it not?\n\nAre the 'Twelve' otherwise engaged? I was given to understand they were your protectors. If you truly believe them your guardians, why do you not repeat the trick that served you so well at Carteneau, and call them down? They will answer--so long as you lavish them with crystals and gorge them on aether. Your gods are no different than those of the beasts--eikons every one. Accept but this, and you will see how Eorzea's faith is bleeding the land dry.",
        ];

        public static Dictionary<string, TrapType> TrapLinkNames = new Dictionary<string, TrapType>()
        {
            { "Bald Trap",          TrapType.Bald },
            { "Bubble Trap",        TrapType.Bubble },
            { "Hiccup Trap",        TrapType.Hiccup },
            { "Ice Trap",           TrapType.Ice },
            { "Invisible Trap",     TrapType.Invisible },
            { "Literature Trap",    TrapType.Literature },
            { "Reverse Trap",       TrapType.Reverse },
            { "Stun Trap",          TrapType.Stun },
            { "Zoom In Trap",       TrapType.Zoom_In },
            { "Zoom Out Trap",      TrapType.Zoom_Out },

            { "Animal Bonus Trap",  TrapType.Literature },
            { "Banana Trap",        TrapType.Ice },
            { "Banana Peel Trap",   TrapType.Ice },
            { "Bonk Trap",          TrapType.Hiccup },
            { "Camera Rotate Trap", TrapType.Zoom_Out },
            { "Chaos Control Trap", TrapType.Stun },
            { "Confuse Trap",       TrapType.Reverse },
            { "Cutscene Trap",      TrapType.Literature },
            { "Deisometric Trap",   TrapType.Zoom_Out },
            { "Depletion Trap",     TrapType.Bald },
            { "Dry Trap",           TrapType.Bald },
            { "Eject Ability",      TrapType.Hiccup },
            { "Exposition Trap",    TrapType.Literature },
            { "Fear Trap",          TrapType.Hiccup },
            { "Freeze Trap",        TrapType.Stun },
            { "Frog Trap",          TrapType.Hiccup },
            { "Frozen Trap",        TrapType.Stun },
            { "Ghost",              TrapType.Invisible },
            { "Ghost Chat",         TrapType.Hiccup },
            { "Ice Floor Trap",     TrapType.Ice },
            { "Invisibility Trap",  TrapType.Invisible },
            { "Jump Trap",          TrapType.Hiccup },
            { "Jumping Jacks Trap", TrapType.Hiccup },
            { "Laughter Trap",      TrapType.Hiccup },
            { "Reversal Trap",      TrapType.Reverse },
            { "Paralyze Trap",      TrapType.Stun },
            { "Paralysis Trap",     TrapType.Stun },
            { "Phone Trap",         TrapType.Literature },
            { "Pixelate Trap",      TrapType.Zoom_In },
            { "Pixellation Trap",   TrapType.Zoom_In },
            { "Poison Mushroom",    TrapType.Hiccup },
            { "Poison Trap",        TrapType.Hiccup },
            { "Possession Trap",    TrapType.Invisible },
            { "Slip Trap",          TrapType.Ice },
            { "Spring Trap",        TrapType.Hiccup },
            { "Spam Trap",          TrapType.Literature },
            { "Tutorial Trap",      TrapType.Literature },
        };
        public static Dictionary<int, int> EnabledTraps = new Dictionary<int, int>();

        public static TrapManager Instance { get; private set; }

        public static TrapExpirationAction ExpirationAction { get; set; } = TrapExpirationAction.Deaths;
        public static int ExpirationAmount { get; set; } = 5;
        public static int TRAP_COOLDOWN = 120;

        public int TrapCooldownTimer = TRAP_COOLDOWN;
        public List<BaseTrapInstance> ActiveTraps = new List<BaseTrapInstance>();
        public Queue<BaseTrapInstance> QueuedTraps = new Queue<BaseTrapInstance>();
        BaseTrapInstance PriorityTrap = null;

        public TrapManager()
        {
            Instance = this;
        }
        public void Update()
        {
            if (!Game.Instance.ArchipelagoManager.Ready)
            {
                return;
            }

            this.ActiveTraps.RemoveAll(trap => this.IsTrapExpired(trap));

            if (this.TrapCooldownTimer > 0)
            {
                this.TrapCooldownTimer--;
                this.TickActiveTraps();
                return;
            }

            // Check Priority Trap for validity, discard if invalid
            if (this.PriorityTrap != null && this.IsTrapValid(this.PriorityTrap.type))
            {
                this.ActiveTraps.Add(this.PriorityTrap);
                this.PriorityTrap.Activate();

                ArchipelagoMessage message = new ArchipelagoMessage(this.PriorityTrap.message);
                Game.Instance.ArchipelagoManager.MessageLog.Insert(0, message);
                Log.Info(this.PriorityTrap.message);
                Audio.Play(Sfx.sfx_glassbreak);
            }

            this.PriorityTrap = null;

            // Dequeue a trap, check for validity, requeue if invalid
            if (this.QueuedTraps.Count > 0)
            {
                BaseTrapInstance newTrap = this.QueuedTraps.Dequeue();
                if (this.IsTrapValid(newTrap.type))
                {
                    this.ActiveTraps.Add(newTrap);
                    newTrap.Activate();

                    Game.Instance.ArchipelagoManager.SendTrapLink(newTrap.type);

                    ArchipelagoMessage message = new ArchipelagoMessage(newTrap.message);
                    Game.Instance.ArchipelagoManager.MessageLog.Insert(0, message);
                    Log.Info(newTrap.message);
                    Audio.Play(Sfx.sfx_glassbreak);

                    this.TrapCooldownTimer = TRAP_COOLDOWN;
                }
                else
                {
                    this.QueuedTraps.Enqueue(newTrap);
                }
            }

            this.TickActiveTraps();
        }

        public void TickActiveTraps()
        {
            foreach (TrapType trapType in Enum.GetValues(typeof(TrapType)))
            {
                bool bActive = this.IsTrapActive(trapType);
                switch (trapType)
                {
                    case TrapType.Bald:
                    {
                        // Handled in modPlayer.cs
                        Player.BaldActive = bActive;
                        break;
                    }
                    case TrapType.Bubble:
                    {
                        BaseTrapInstance trap = this.ActiveTraps.Find(trap => trap.type == TrapType.Bubble);
                        if (trap != null && (DateTime.Now - trap.activationTime).TotalSeconds >= 3.0f)
                        {
                            this.ActiveTraps.Remove(trap);
                        }
                        break;
                    }
                    case TrapType.Hiccup:
                    {
                        BaseTrapInstance trap = this.ActiveTraps.Find(trap => trap.type == TrapType.Hiccup);
                        if (trap != null && (trap.activationTime - DateTime.Now).TotalSeconds <= 0.0f)
                        {
                            Player.HiccupActive = true;
                            trap.activationTime = DateTime.Now;
                            trap.activationTime = trap.activationTime.AddSeconds(3.0f + (Random.Shared.NextDouble() * 12.0f));
                        }

                        break;
                    }
                    case TrapType.Ice:
                    {
                        Player.IceActive = bActive;

                        break;
                    }
                    case TrapType.Invisible:
                    {
                        Player.InvisibleActive = bActive;

                        break;
                    }
                    case TrapType.Literature:
                    {
                        BaseTrapInstance trap = this.ActiveTraps.Find(trap => trap.type == TrapType.Literature);
                        if (trap != null && (DateTime.Now - trap.activationTime).TotalSeconds >= 6.0f)
                        {
                            this.ActiveTraps.Remove(trap);
                        }

                        break;
                    }
                    case TrapType.Reverse:
                    {
                        Player.ReverseActive = bActive;

                        break;
                    }
                    case TrapType.Stun:
                    {
                        Player.StunActive = bActive;
                        // Mostly handled in modPlayer.cs
                        BaseTrapInstance trap = this.ActiveTraps.Find(trap => trap.type == TrapType.Stun);
                        if (trap != null && (DateTime.Now - trap.activationTime).TotalSeconds >= 3.0f)
                        {
                            this.ActiveTraps.Remove(trap);
                        }

                        break;
                    }
                    case TrapType.Zoom_In:
                    {
                        Player.ZoomInActive = bActive;

                        break;
                    }
                    case TrapType.Zoom_Out:
                    {
                        Player.ZoomOutActive = bActive;

                        break;
                    }
                }
            }
        }

        public void AddTrapToQueue(TrapType type, string message)
        {
            this.QueuedTraps.Enqueue(new BaseTrapInstance(type, message, ExpirationAction, ExpirationAmount));
        }

        public void SetPriorityTrap(TrapType type, string message)
        {
            this.PriorityTrap = new BaseTrapInstance(type, message, ExpirationAction, ExpirationAmount, true);
        }

        public bool IsTrapValid(TrapType type)
        {
            if (this.IsTrapActive(type))
            {
                return false;
            }

            World? world = Game.Instance.GetWorld();
            if (world == null)
            {
                return false;
            }

            Player? player = world.Get<Player>();
            if (player == null)
            {
                return false;
            }

            if (!player.IsTrappable())
            {
                return false;
            }

            switch (type)
            {
                case TrapType.Bald:
                {
                    break;
                }
                case TrapType.Bubble:
                {
                    break;
                }
                case TrapType.Hiccup:
                {
                    break;
                }
                case TrapType.Ice:
                {
                    break;
                }
                case TrapType.Invisible:
                {
                    break;
                }
                case TrapType.Literature:
                {
                    if (Game.Instance.ArchipelagoManager.LiteratureLog.Count > 0)
                    {
                        return false;
                    }

                    break;
                }
                case TrapType.Reverse:
                {
                    break;
                }
                case TrapType.Stun:
                {
                    break;
                }
                case TrapType.Zoom_In:
                {
                    if (this.IsTrapActive(TrapType.Zoom_Out))
                    {
                        return false;
                    }
                    break;
                }
                case TrapType.Zoom_Out:
                {
                    if (this.IsTrapActive(TrapType.Zoom_In))
                    {
                        return false;
                    }
                    break;
                }
            }

            return true;
        }

        public bool IsTrapActive(TrapType type)
        {
            return this.ActiveTraps.Where(item => item.type == type).Count() > 0;
        }

        public bool IsTrapExpired(BaseTrapInstance trap)
        {
            switch (trap.expirationAction)
            {
                case TrapExpirationAction.Screens:
                {
                    return trap.trackedScreens.Count > trap.expirationAmount;
                }
                case TrapExpirationAction.Deaths:
                {
                    return trap.trackedDeaths >= trap.expirationAmount;
                }
            }

            return false;
        }

        public void AddDeathToActiveTraps()
        {
            foreach (BaseTrapInstance trap in this.ActiveTraps)
            {
                trap.trackedDeaths += 1;
            }
        }

        public void AddScreenToActiveTraps(string screen)
        {
            foreach (BaseTrapInstance trap in this.ActiveTraps)
            {
                trap.trackedScreens.Add(screen);
            }
        }

        public void Reset()
        {
            this.ActiveTraps.Clear();
            this.QueuedTraps.Clear();
            this.PriorityTrap = null;
        }
    }
}
