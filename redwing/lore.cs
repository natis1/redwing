using angleintegration;
using Language;

namespace redwing
{
    public static class lore
    {
        public static void createLore(bool overrideBlackmoth, bool forceEnglishLore)
        {
            log("Preparing to setup lore. Force english lore is " + forceEnglishLore + " and is english is " + isEnglish());
            // ReSharper disable once InvertIf because it will look dumb in the future.
            if (forceEnglishLore || isEnglish())
            {
                setupLoreEN();
                if (overrideBlackmoth)
                {
                    setupBlackmothLoreEN();
                }
            }
        }
        
        public static bool isEnglish()
        {
            LanguageCode locale = Language.Language.CurrentLanguage();
            return (locale == LanguageCode.EN || locale == LanguageCode.EN_AU || locale == LanguageCode.EN_CA
                    || locale == LanguageCode.EN_CB || locale == LanguageCode.EN_GB || locale == LanguageCode.EN_IE
                    || locale == LanguageCode.EN_JM || locale == LanguageCode.EN_NZ || locale == LanguageCode.EN_TT
                    || locale == LanguageCode.EN_US || locale == LanguageCode.EN_ZA);
        }

        private static void setupLoreEN()
        {
            //angleint.addLanguageString(new language_string("Sheet", "Key", "String"));
            angleint.addLanguageString(new language_string("Lore Tablets", "RANDOM_POEM_STUFF",
                "Can one say with any certainty that Redwing lore is any less valid than vanilla lore? " +
                "On the surface it seems like Team Cherry have some authority over the Hollow Knight universe, " +
                "since they were the ones to originally create it. " +
                "However, I firmly believe that artists have no right to control how their works are interpreted. " +
                "With that in mind I must confess that Redwing’s lore is my real interpretation of the Hollow Knight universe " +
                "and not, as it may seem, a reimagining to fit a mod thematically. " +
                "The lore came first and mod second. " +
                "Thus, the text you see here is what I truly believe happened in Hallownest, at least the Hallownest of my mind.<page>" +
                
                "For there is no one Hallownest but millions, " +
                "each stored in the brains of people who have played Hollow Knight, and each one different from all the others. " +
                "The vanilla game is a few people attempting to transcribe just one of these worlds into a game. " +
                "The information inside is not any more or less real or factual than any other Hallownest, including my own.<page>" +
                
                "I have no control over how you interpret this lore, and I don’t want any. " +
                "Everything inside is factual. " +
                "Read through it and decide which facts you like and dislike, or outright reject the presuppositions of. " +
                "Create, in your mind, the Hallownest you want to imagine. " +
                "Or don’t. " +
                "After all, I have no control over how you interpret this lore."));

            
            angleint.addLanguageString(new language_string("General", "PROLOGUE_EXCERPT_01",
                "In nightmares they think of you with adoration and respect"));
            angleint.addLanguageString(new language_string("General", "PROLOGUE_EXCERPT_02",
                "To the beasts and bugs there seemed no change which you could not effect."));
            angleint.addLanguageString(new language_string("General", "PROLOGUE_EXCERPT_03",
                "The darkest void of your soul could not contain the fire and light that beamed."));
            angleint.addLanguageString(new language_string("General", "PROLOGUE_EXCERPT_04",
                "A light strong enough to kill an evil of which bugs could not have dreamed."));
            angleint.addLanguageString(new language_string("General", "GAME_TITLE", "Redwing"));
            angleint.addLanguageString(new language_string("General", "PROLOGUE_EXCERPT_AUTHOR",
                "- From ‘The Legacy of Hallownest’ by Grimmkin Lynn"));
            angleint.addLanguageString(new language_string("General", "VERSION_NUMBER", "1.1.0"));
            angleint.addLanguageString(new language_string("Enemy Dreams", "HORNET_GREENPATH_2",
                "This strength... for a creature so weak."));
            angleint.addLanguageString(new language_string("Enemy Dreams", "HORNET_GREENPATH_1",
                "What trickery is this little ghost?"));
            angleint.addLanguageString(new language_string("Enemy Dreams", "HORNET_GREENPATH_3",
                "How can you fight like this?"));
            angleint.addLanguageString(new language_string("Hornet", "HORNET_SPIDER_TOWN_01",
                "So you've slain the Beast... and you head towards that fated goal.<page>" +
                "I am sorry I ever tried to stand in your way but I sought to protect her.<page>" +
                "...You might think me stern but I'm not completely cold.<page>" +
                "We do not choose our mothers, or the circumstance into which we are born. Despite all the ills of this world, I'm sad for her loss.<page>" +
                "It's quite a debt I owed. Only in allowing her to pass, and taking the burden of the future in her stead, can I begin to repay it."));
            angleint.addLanguageString(new language_string("Hornet", "HORNET_DOOR_UNOPENED",
                "You never cease to amaze me. You've taken on the fate of this world like it’s nothing.<page>" +
                "Even breaking the Dreamer's seals would be considered an impossible task for anyone else, but to hold the power of the entire kingdom, that casts you as something rather exceptional."));
            angleint.addLanguageString(new language_string("Hornet", "HORNET_SPIDER_TOWN_DREAM",
                "...Mother... Forgive my inaction... but the knight gives us another path..."));
            angleint.addLanguageString(new language_string("Hornet", "HORNET_FINAL_BOSS",
                "Do it! Make your choice. My thread will only hold so long.<page>" +
                "Take the strength of the world for yourself or face the heart of the infection!"));
            angleint.addLanguageString(new language_string("Hornet", "HORNET_ABYSS_ASCENT_01",
                "Ghost. I see you've faced the place of your birth, and now you hold its shadowy substance within you.<page>" +
                "Though our strength is born of similar source, that part of you, that light, I do not share.<page>" +
                "Funny then, that such radiance could give me hope. Within it, I see a break from this stasis.<page>" +
                "A difficult journey you would face, but a choice it can create. Take our world’s fire or restore it to greatness."));
            angleint.addLanguageString(new language_string("Hornet", "HORNET_ABYSS_DREAM",
                "...It faced the void, and remains uncorrupted... Could it unite darkness and light?"));
            angleint.addLanguageString(new language_string("Hornet", "HORNET_ABYSS_REPEAT",
                "I’d urge you to sacrifice that flame for our kingdom, but the decision rests with you."));
            angleint.addLanguageString(new language_string("Hornet", "HORNET_PRE_FINAL_BATTLE_REPEAT",
                "Redwing, you possess the strength to enact an end of your choosing. Would you consume our birth-cursed sibling, or would you destroy it?"));
            angleint.addLanguageString(new language_string("Hornet", "HORNET_FOUNTAIN_1",
                "Hello again, little ghost.<page>" +
                "I'm normally quite perceptive. And yet I greatly underestimated, though I've since learned the truth.<page>" +
                "You've seen beyond this kingdom's bounds. Yours is resilience born of void and fire.<page>" +
                "It's no surprise then you've managed to reach the heart of this world. In so doing, you shall know the being that tore it apart."));
            angleint.addLanguageString(new language_string("Hornet", "HORNET_FOUNTAIN_2",
                "If, knowing that truth, you'd still choose to keep Hallownest alive, seek the Grave in Ash and the mark it would grant to one like you."));
            angleint.addLanguageString(new language_string("Cornifer", "FUNGAL_WASTES_BOUGHT",
                "There's a potent odour about these caverns and far worse for me I suspect. My trunk is quite sensitive you see, but you seem unaffected.<page>" +
                "Perhaps such scents are inconsequential to you."));
            angleint.addLanguageString(new language_string("Cornifer", "FUNGAL_WASTES_GREET",
                "Ahh my short friend, you've caught me at the perfect time. I'm just about finished charting these noxious caverns.<page>" +
                "Very territorial types make their homes within this area. I'd suggest avoiding them where possible.<page>" +
                "Further below some strange thin creatures gave me quite a scare. In spite of their primitive nature, they’re quite intelligent.<page>" +
                "In my youth I'd have braved their caves but I fear my matured physique wouldn't be able to outrun them were they to turn violent."));
            angleint.addLanguageString(new language_string("Cornifer", "CORNIFER_MEET_DP",
                "Hh-hello. I'm Cornifer. I'm a m-mapper by trade. I've tried to chart the dense nest beneath here b-b-but it's proving too dangerous for a bug like me.<page>" +
                "Vicious little creatures burst out all o-over the place and the passages are a dark, twisting maze. Even with my good head for direction, I-I-I've had enough<page>" +
                "You have an air of strength about you and c-c-can probably handle yourself, would you like to buy m-my meagre map?"));
            angleint.addLanguageString(new language_string("Cornifer", "DEEPNEST_GREET",
                "This p-place gives me the creeps. Vicious little creatures burst out all o-over the place and the passages are a dark, twisting maze.<page>" +
                "You have an air of strength about you and c-c-can probably handle yourself, would you like to buy my map?"));
            angleint.addLanguageString(new language_string("Cornifer", "MINES_BOUGHT",
                "I jabbed myself on one of those crystals back there. Awful sharp they are and all over the place. You look like you can handle yourself, but do be careful."));
            angleint.addLanguageString(new language_string("Hunter", "HUNTER_DREAM",
                "It is so much stronger stronger than me... does it know that? Will it return to hunt me...? I hope not."));
            angleint.addLanguageString(new language_string("Relic Dealer", "RELICDEALER_DREAM_FOUNTAIN",
                "...Who were you mystery knight?... Why dark secrets do you hide that you only appear in this fountain?.."));
            angleint.addLanguageString(new language_string("Relic Dealer", "RELICDEALER_DREAM",
                "...I’ve seen this short one before... maybe in a dream long ago?.."));
            angleint.addLanguageString(new language_string("Relic Dealer", "RELICDEALER_DUNG",
                "Ech! I cannot have you coming into my store stinking like that!<page>" +
                "These relics have been through enough and I’ve spent far too long cleaning them! Come back when you’ve cleaned yourself up a bit!"));
            angleint.addLanguageString(new language_string("Relic Dealer", "RELICDEALER_TALK_REPEAT",
                "I must be getting back to my work. Relics need cleaning. Texts need deciphering. Why not try and find some more for me?"));
            angleint.addLanguageString(new language_string("Relic Dealer", "RELICDEALER_SEAL_3",
                "A Hallownest Seal, eh? Give it here.<page>" +
                "You’re quite talented at finding these. I just wish I knew who the great five were."));
            angleint.addLanguageString(new language_string("Relic Dealer", "RELICDEALER_TALK_2",
                "Admiring my shop are you? Well I'm no squatter.<page>" +
                "This place was dead empty when I moved in and there's no one left alive who'd lay claim to the tower.<page>" +
                "I’d prefer to keep it that way, so please, if you wish to find a place to live, look elsewhere.<page>" +
                "I’d rather be with just my relics. I'm not after neighbours."));
            angleint.addLanguageString(new language_string("Relic Dealer", "RELICDEALER_NAILSMITH",
                "That nail you bear looks mighty blunt. There're cracks all through it. It's as much a relic as the objects in my shop!<page>" +
                "You look quite strong but your strength seems hampered by your weak weapon. It may be an idea to hunt down that old Nailsmith.<page>" +
                "His hut's on the outskirts of the city. Not that far from here. He might add some heat to your flame, and some sting to your blade."));
            angleint.addLanguageString(new language_string("Relic Dealer", "RELICDEALER_FOUNTAIN",
                "Oh? What are you doing out in this miserable downpour?<page>" +
                "Impressive fountain isn't it? I'm sure we'd be able to appreciate it more if we weren't both getting drenched.<page>" +
                "That knight up there was an important one. The plaque here mentions its 'sacrifice,' which carries something of an ominous tone.<page>" +
                "Probably met some sort of horrible end, but was the knight's goal achieved in the process?<page>" +
                "In all the relics I've collected, I've yet to find a clue as to what that was."));
            angleint.addLanguageString(new language_string("Minor NPC", "KING_FINAL_WORDS",
                "...Soul of Wyrm. Soul of Root. Infinitely radiant..."));
            angleint.addLanguageString(new language_string("Minor NPC", "MINER_DREAM_2",
                "... how much longer... KILL IT... how much longer... DANGEROUS... how much longer... KILL IT... how much longer... KILL THE FIERY ONE...<page>" +
                "...how much longer... must I dig...?"));
            angleint.addLanguageString(new language_string("Minor NPC", "MASK_MAKER_UNMASK2",
                "Has it witnessed that truth most tragic? The Wyrm’s desperate failing.<page>" +
                "Now this Kingdom's death becomes the Wyrm's legacy."));
            angleint.addLanguageString(new language_string("Minor NPC", "CLOTH_QG_GREET",
                "Ahhh tiny fire being. Then, you too come to fight that other tribe?<page>" +
                "A deadly bunch they are that roost within these glades. I'd once've stayed well clear of them, but your actions have shown me the truth of it. We must face down our fears or be defeated by them.<page>" +
                "If I somehow make it through, we can swap stories of our adventures. I'd look forward to that!"));
            angleint.addLanguageString(new language_string("Minor NPC", "CLOTH_TRAMWAY_SAVED",
                "Enough! I cannot wallow in my weakness. I must take strength from your example!<page>" +
                "You fight with an impossible dexterity. You fearlessly incinerate all in your way. I'll try my best to show half your courage."));
            angleint.addLanguageString(new language_string("Minor NPC", "JINN_TALK_02",
                "...Its body, so soft... Fragile... Inferior built. Yet it seems impossible to break...<page>" +
                "Does it take hurt...?"));
            angleint.addLanguageString(new language_string("Minor NPC", "QUEEN_MEET",
                "Oh! One arrives. Far it walks to find me. Did it seek my aid? Or did the path carry it by chance to so pertinent a place?<page>" +
                "It is true. True, that you were awaited. No. Perhaps that is inaccurate. True one like you was awaited.<page>" +
                "Never could I have anticipated that the one would be so powerful.<page>" +
                "Regardless, I have a gift, held long for one who could help. Half of a whole. When united, immense power granted, and on the path ahead, power it will need."));
            angleint.addLanguageString(new language_string("Minor NPC", "DUNG_UNDERGROUND_2",
                "My cave may seem a far cry from the finery of the palace but even still the memories of my former comrades, and our...king, remain fresh in my mind.<page>" +
                "The Pale Court was a wondrous place full of the age's greatest heroes. Ahh! Looking at you now, I can easily imagine you standing above them."));
            angleint.addLanguageString(new language_string("Minor NPC", "BIGCAT_SHADECHARM",
                "Ohhhmmmm... Tiny thing... It evolves farther beyond that Wyrm. Such union in a single being. A strength before unseen. Would it too challenge nature? It could perhaps defeat it."));
            angleint.addLanguageString(new language_string("Minor NPC", "WHITE_DEFENDER_OUTRO_3",
                "My King... Is something wrong? Why do you look so cagey?"));
            angleint.addLanguageString(new language_string("Minor NPC", "WHITE_DEFENDER_OUTRO_4",
                "Was I wrong to trust you, Pale Wyrm? Why have you betrayed us?"));
            angleint.addLanguageString(new language_string("Minor NPC", "WHITE_DEFENDER_OUTRO_5",
                "Red god! Your strength and nobility set you above our King. Perhaps you could rule this land and restore to us the light and beauty we lost! How I would love to be your knight..."));
            angleint.addLanguageString(new language_string("Minor NPC", "QUEEN_MEET_REPEAT",
                "Prepare well, but don't dally. Were the Vessel to break prematurely, I fear the destructive force of the corrupted light would unleash a massive rage and power."));
            angleint.addLanguageString(new language_string("Minor NPC", "GIRAFFE_DREAM",
                "This little creature looks tasty. If it wasn’t so strong I’d eat it. The others around here were awfully bland."));
            angleint.addLanguageString(new language_string("Minor NPC", "QUEEN_DREAM",
                "Of course it can access a mind. What can’t it do?"));
            angleint.addLanguageString(new language_string("Minor NPC", "DUNG_DEFENDER_3",
                "We knights defend against the physical, but a formless enemy. How to defeat such a foe?<page>" +
                "When we sought help most, our King vanished. And so, we were brought low... I wonder why he left...<page>" +
                "Bah! I shouldn't be so morose. You've invigorated me. Tis' truly a delight to meet one whose strength can match my own."));
            angleint.addLanguageString(new language_string("Minor NPC", "BIGCAT_TALK_01",
                "This falling ash is moult. The Wyrm's corpse decaying. Endless. Rmm... Serene. Sad.<page>" +
                "With its like gone, the world may finally see peace."));
            angleint.addLanguageString(new language_string("Minor NPC", "BIGCAT_TALK_03",
                "For quiet retreat did I climb up here, away from spitting creatures. Ormmph... Yes. High up. Away from simple minds, lost to light.<page>" +
                "Theirs is a different kind of unity. Rejection of the Wyrm's attempt at order.<page>" +
                "I resist the light's allure. Union it may offer, but a loss of control over one’s destiny.<page>" +
                "Why should I trust my physical form with the will of a fallible being?"));
            angleint.addLanguageString(new language_string("Minor NPC", "KING_ABYSS_01", "No hopes to dream."));
            angleint.addLanguageString(new language_string("Minor NPC", "KING_ABYSS_02", "No fire to burn."));
            angleint.addLanguageString(new language_string("Minor NPC", "KING_ABYSS_03", "No light to shine."));
            angleint.addLanguageString(new language_string("Minor NPC", "KING_ABYSS_04", "No mind to resist."));
            angleint.addLanguageString(new language_string("Minor NPC", "KING_ABYSS_FINAL",
                "You shall absorb the blinding light which plagues my reign.<page>" +
                "You are the Vessel.<page>" +
                "You are the Hollow Knight."));
            angleint.addLanguageString(new language_string("Minor NPC", "BIGCAT_KING_BRAND",
                "Tiny thing... Oh hmm... The mark of Wyrm you bear. Is it fire you seek? Or to save this ruin?<page>" +
                "That choice is yours. Always the smallest creatures that wield the largest powers."));
            angleint.addLanguageString(new language_string("Minor NPC", "BRETTA_DREAM_BED",
                "Red Wanderer...don't be shy....cold outside....bed is soft..."));
            angleint.addLanguageString(new language_string("Minor NPC", "BRETTA_DIARY_2",
                "The Red Saviour Returns<br>Long had they remained apart and the village, once so warm, now grew cold. The maiden felt the well of grief. It gripped fierce about her lonely shell.<page>" +
                "And suddenly, as though her tragic state was sensed, the saviour returned, standing tall, glowing bright. Below shining horns, eyes welled crimson, glistening, eyes only for the maiden long missed, long desired.<page>" +
                "Her saviour leaned close, sat beside, perfectly composed. In that charged, breathless moment not a word needed be spoken, <page>" +
                "The maiden's shell felt suddenly tight. Her claws curled. No glance was shared, no claws touched, just perfect, aching love shared in silence, together..."));
            angleint.addLanguageString(new language_string("Minor NPC", "LITTLE_FOOL_CHALLENGE",
                "You've placed your mark, and the great gate has opened. Go on! Step into the Colosseum and burn your way to victory."));
            angleint.addLanguageString(new language_string("Minor NPC", "LITTLE_FOOL_GREET",
                "Welcome back, little fire god!"));
            angleint.addLanguageString(new language_string("Minor NPC", "EMILITIA_DREAM",
                "I fear this one. It has a strength inside unlike any I have ever witnessed. I want to outlive you too, as I did all the others."));
            angleint.addLanguageString(new language_string("Minor NPC", "CLOTH_FUNG_DREAM",
                "Am I in over my head? Even these shroom creatures almost did me in and far nastier things await further down.<page>" +
                "Curse me and my foolish bravado! If I could shed my pride, I'd be asking this powerful little fighter for help."));
            angleint.addLanguageString(new language_string("Minor NPC", "BIGCAT_INTRO",
                "Ohrm... Tiny thing. You climb high. Seek then knowledge of these lands?<page>" +
                "This ashen place is grave of Wyrm. Once told, it came to die. But what is death for that ancient being? More transformation methinks.<page>" +
                "This failed kingdom is product of its death, and the being spawned from that event."));
            angleint.addLanguageString(new language_string("Minor NPC", "MOSS_CULTIST_DREAM_INFECTED",
                "...Blazing...Bright...Empty?..."));
            angleint.addLanguageString(new language_string("Minor NPC", "BRETTA_DIARY_1",
                "The Red Saviour<br>The maiden woke in darkness. Confused she reached out. Sharp brambles jabbed at all sides. Burning acid bubbled close below. What nightmare had led her here? What hope of survival remained?<page>" +
                "Doomed she thought herself and to despair she fell, until a light bloomed far in the distance, a bright, glowing spot fast approaching. It swept majestic about the thorns, leapt above burning waters and dove towards the maiden.<page>" +
                "Coming close, the form revealed at last, a beautiful being, sharp horns gleaming white, a scarlet cloak. Arms reached out for the damsel, gathering her up, grip firm..."));
            angleint.addLanguageString(new language_string("Minor NPC", "BRETTA_DIARY_3",
                "The Red Saviour in Darkness<br>Troubled dreams beset the maiden. Her saviour gone, consumed below. Now her only companion the cold wind, moaning at her door. Her heart fluttered with sudden fear...<page>" +
                "Then still. A sudden calm. Why? A presence. A figure close behind.<page>" +
                "She doesn't dare look, doesn't dare move, fearful the slightest action would break the spell. She knew the presence at her bed, knew the calm only they could bring.<page>" +
                "Her red saviour, now protector, standing tall beside, powerful, radiant..."));
            angleint.addLanguageString(new language_string("Minor NPC", "BRETTA_DIARY_4",
                "The Grey Prince<br>Heaving heavy breaths; flush from the exertion of battle; the figure emerged from the well. Triumph was his and trophy he bore to prove it, the fearsome skull of his vanquished opponent.<page>" +
                "Startling warning he brought to the village, one that rung so true: Their red saviour, thought a hero by the bugs, was a vulture and this shrouded grey prince was in truth the hero deserved.<page>" +
                "With honour and humility he recounted his quest below, his epic journey of one purpose, to find her, to protect her, his grey maiden, his partner in darkness..."));
            angleint.addLanguageString(new language_string("Minor NPC", "MASK_MAKER_DREAM",
                "...Does it know of the face that hides beneath? Such remarkable contrast between light and dark...How can it be..."));
            angleint.addLanguageString(new language_string("Minor NPC", "QUEEN_REPEAT_KINGSOUL",
                "Ahh! So it bears the tool, nearly complete. Such strength, such resolve, such dedication! No mere vessel could wield such power. Even the presence of my beloved Wyrm pails in comparison.<page>" +
                "The Radiant Heart... What is at the center of it I wonder? If it wishes to use it, it should seek out that place. That place where it was born, where it died, where it began..."));
            angleint.addLanguageString(new language_string("Minor NPC", "KING_ABYSS_FINAL_A",
                "You shall absorb the blinding light which plagues my reign."));
            angleint.addLanguageString(new language_string("Minor NPC", "QUEEN_TALK_01",
                "Within my roots, the weakening of the Vessel I plainly feel. Only two obvious outcomes there are from such a thing.<page>" +
                "The first is inevitable on current course, regression, all minds relinquished to the corruption.<page>" +
                "The second I find preferable, and would seek your aid in its occurance, destruction.<page>" +
                "I implore you, destroy the Vessel. Its supposed strength was ill-judged. No void could contain the fire of the sun. None except you. You have the strength to take the kingdom’s flames for yourself and let us bugs rest in death."));
            angleint.addLanguageString(new language_string("Minor NPC", "QUEEN_TALK_02",
                "Normally I’d offer fair warning. After all, the Vessel may itself be weak, but it is much empowered by that force within.<page>" +
                "But you clearly have what it takes to take it on and claim its power if that is what you choose to do."));
            angleint.addLanguageString(new language_string("Ghosts", "XERO_INSPECT",
                "I mourn those who turn against the King. And those who lost their lives despite staying loyal."));
            angleint.addLanguageString(new language_string("Ghosts", "MARKOTH_DEFEAT",
                "Never... have I been defeated in combat.<page>" +
                "I can... see myself there, still sleeping. How long have I been hidden here?<page>" +
                "Here at the edge of the world, no-one could find me... except you.<page>" +
                "Warriors, knights, kings, even time itself... they have no power over me. Only you.<page>" +
                "You are the fire... come to consume me."));
            angleint.addLanguageString(new language_string("Dreamers", "DREAMERS_EC2_1D",
                "But the Vessel failed.<br>The plague consumes all.<br>They must be undone."));
            angleint.addLanguageString(new language_string("Dreamers", "DREAMERS_EC2_1",
                "It would break the Seals.<br>They cannot be undone.<br>They must be undone.<page>" +
                "An empty vessel to bring salvation.<br>A flaming vessel to bring life.<br>A radiant vessel to bring dreams.<page>" +
                "What then is this? Only a weak thing. It would harm the knight, harm the seals. It shall be cast away."));
            angleint.addLanguageString(new language_string("Dreamers", "DREAMERS_EC2_1A",
                "It would break the Seals.<br>They cannot be undone.<br>They must be undone."));
            angleint.addLanguageString(new language_string("Dreamers", "DREAMERS_EC2_1B",
                "An empty vessel to bring salvation."));
            angleint.addLanguageString(new language_string("Dreamers", "DREAMERS_EC2_1C",
                "A flaming vessel to bring life."));
            angleint.addLanguageString(new language_string("Dreamers", "DREAMERS_EC2_1D",
                "A radiant vessel to bring dreams."));
            angleint.addLanguageString(new language_string("Dreamers", "DREAMERS_EC2_1E",
                "What then is this? Only a weak thing. It would harm the knight, harm the seals. It shall be cast away."));
            angleint.addLanguageString(new language_string("Dreamers", "MONOMON_CONVO_6",
                "...Take our flame and leave..."));
            angleint.addLanguageString(new language_string("Dreamers", "DREAMERS_EC1_1",
                "Would it seek to take our flame?"));
            angleint.addLanguageString(new language_string("Dreamers", "DREAMERS_EC1_2",
                "The seals cannot be undone."));
            angleint.addLanguageString(new language_string("Dream Witch", "WITCH_QUEST_1",
                "Hm. You are practiced in the way of dreams, yet you lack experience with the Dream Nail. Collect 100 Essence and visit me again and I will share what little wisdom I have."));
            angleint.addLanguageString(new language_string("Dream Witch", "WITCH_QUEST_6",
                "Ahhh, the Dream Nail is so radiant! Keep it up. Return to me once you have collected 1200 Essence."));
            angleint.addLanguageString(new language_string("Dream Witch", "WITCH_QUEST_7",
                "Ahhh, the Dream Nail is so radiant! Keep it up. Return to me once you have collected 1500 Essence."));
            angleint.addLanguageString(new language_string("Dream Witch", "WITCH_FINAL_1",
                "So much Essence... So bright.... You truly are the Wielder my tribe so long has dreamed of.<page>" +
                "The folk of my tribe were born from a light. Light similar to Essence, similar to that powerful blade, though much brighter still.<page>" +
                "They were content to bask in that light and honoured it... for a time.<page>" +
                "But another light appeared in our world... A wyrm that used its power to dispel all other gods.<page>" +
                "How fickle my ancestors must have been. They forsook the light that spawned them the second it disappeared. Turned their backs to it... Forgot it even."));
            angleint.addLanguageString(new language_string("Dream Witch", "WITCH_DREAM1",
                "To cast you away into this space between body and soul...<page>" +
                "But you’ve been here before, and you shall prevail.<page>" +
                "For you cannot accept their judgement and fade slowly away.<page>" +
                "You’re strong enough to break out of this on your own. Before you go, though take the weapon before you as a gift, and leave this sad forgotten dream to rot."));
            angleint.addLanguageString(new language_string("Dream Witch", "WITCH_DREAM_FALL",
                "Though you may fall, your will shall carry you forward.<page>" +
                "A dream is endless, but a Kingdom is not.<page>" +
                "The power to wake this world from its slumber… or take it for yourself...you only need to reach out."));
            angleint.addLanguageString(new language_string("Dream Witch", "WITCH_REWARD_1",
                "Hmm, already you've collected 100 Essence. Quick work! Collecting these remnants is familiar to you, isn’t it?<page>" +
                "How foolish of the Dreamers to try to bury you in that old dream. You must frighten them! Or perhaps, being prisoners themselves, they desired your company?<page>I" +
                "n any case, you still have a long way to go. Take this old trinket as encouragement from me, and return when you have collected 200 Essence."));
            angleint.addLanguageString(new language_string("Nailsmith", "NAILSMITH_MEET_3",
                "After a smith are you? Well you've found one. I'm not much for talk, but if its a flame to be rekindled, you’ve come to the right place.<page>" +
                "Your own firenail has nearly fallen apart and lost much of its original power. Still, anything can be refined with enough effort and skill."));
            angleint.addLanguageString(new language_string("Nailsmith", "NAILSMITH_KILL_REPEAT",
                "Do not hesitate. I beg you. Ignite me! I want to taste that blade's burning edge."));
            angleint.addLanguageString(new language_string("Nailsmith", "NAILSMITH_OFFER_ORE",
                "I see you have some Pale Ore. A rare, fine metal, that. Give me the ore and some Geo for my efforts, and I'll blend it into your firenail to make it stronger."));
            angleint.addLanguageString(new language_string("Nailsmith", "NAILSMITH_OFFER",
                "If you'd like, I can reforge your firenail. It'll make your flames far more deadly."));
            angleint.addLanguageString(new language_string("Nailsmith", "NAILSMITH_KILL",
                "Glorious, I have always wanted to forge a Pure firenail. My work in this lifetime comes to an end. My only remaining desire is to see and feel the heat strike true!<page>" +
                "I beg you, cut me down. As my final moment in life, I want to taste the warmth.<page>" +
                "After all this time, all this toil... haven't I earned it?"));
            angleint.addLanguageString(new language_string("Nailsmith", "NAILSMITH_COMPLETE_1",
                "There we go. The reforging is complete.<page>" +
                "I've brightened the flame inside your nail. You'll find it much stronger than it used to be.<page>" +
                "Head out there and test its strength against your foes."));
            angleint.addLanguageString(new language_string("Nailsmith", "NAILSMITH_COMPLETE_2",
                "There we go. The reforging is complete.<page>" +
                "I've added a channel to your firenail. It should better expose the Nightmare Flames inside.<page>" +
                "Head out there and test its blade against your foes."));
            angleint.addLanguageString(new language_string("Nailsmith", "NAILSMITH_COMPLETE_3",
                "Here we are. The reforging is complete.<page>" +
                "I've added spirals to your firenail. A very tricky task it was. You'll find it cuts hotter than ever before.<page>" +
                "Off you go now. Burn your way forward."));
            angleint.addLanguageString(new language_string("Nailsmith", "NAILSMITH_COMPLETE_4",
                "It's done. The reforging is done.<page>" +
                "Your firenail fully exposes the Nightmare inside. Its form is as perfect as it could ever be.<page>" +
                "In only your weapon have I seen such power. Finally, I behold the majesty of a perfect firenail. <page>" +
                "To think this moment has come upon me so soon...<page>" +
                "...I... I must step outside a moment..."));
            angleint.addLanguageString(new language_string("Nailsmith", "NAILSMITH_DREAM_INTERIOR",
                "...To forge the perfect weapon..."));
            angleint.addLanguageString(new language_string("Nailsmith", "NAILSMITH_NEED_ORE1",
                "If you bring me a piece of Pale Ore, I can forge it into your firenail and make it stronger still."));
            angleint.addLanguageString(new language_string("Nailsmith", "NAILSMITH_NEED_ORE2",
                "If you bring me two pieces of Pale Ore, I can forge it into your firenail and make it stronger still."));
            angleint.addLanguageString(new language_string("Nailsmith", "NAILSMITH_NEED_ORE3",
                "If you bring me three pieces of Pale Ore, I can forge it into your firenail and fully expose its power."));
            angleint.addLanguageString(new language_string("Quirrel", "QUIRREL_GREENPATH_3",
                "Your firenail looks a fine instrument, but it's showing signs of wear. A few repairs could work wonders towards enhancing your power<page>" +
                "...I just worry you may soon meet dangers the surface world can't match."));
            angleint.addLanguageString(new language_string("Quirrel", "DREAM_ARCHIVE_INTERIOR",
                "To think, such a small creature could have this much power."));
            angleint.addLanguageString(new language_string("Quirrel", "DREAM_QUEENSTATION",
                "Even those great stags bowed to Hallownest's King. What control he must have held."));
            angleint.addLanguageString(new language_string("Quirrel", "QUIRREL_MINES_1",
                "Ahh, quite a view, no?<page>" +
                "I'm not surprised you survived the trek through these shimmering caverns.<page>" +
                "There's an air of strength about you. In spite of your small stature, it’s quite intimidating."));
            angleint.addLanguageString(new language_string("Quirrel", "QUIRREL_MANTIS_01",
                "Hello again! I suppose you've already met with the tribe of this village, hmm? They seem a little distrustful of strangers... to put it lightly.<page>" +
                "They're not brutes though, no. The sickness in the air that clouds the mind of other beasts... they resist. They’re not intelligent as much as stubborn, and their lethal traditions help them survive.<page>" +
                "I've some words of advice, my friend. If you plan to challenge the lords of this tribe, you’ll have an easier time with a stronger firenail.<page>" +
                "There is a city nearby, the old capital of Hallownest. I've heard a smith resides there. Seek the old bug out and you may find progress that much easier."));
            angleint.addLanguageString(new language_string("Quirrel", "QUIRREL_TEMPLE_4",
                "To persevere in this ruin, I fear your damaged nail  alone may leave you underpowered. Though that's no problem! One only has to look around.<page>" +
                "Plenty have come before us and most have met their grisly end, many more equipped, though perhaps not as skilled as you.<page>" +
                "I'm sure they wouldn't mind were a fellow explorer to relieve them of their tools. It's a kindness really. The dead shouldn't be burdened with such things."));
            angleint.addLanguageString(new language_string("Quirrel", "QUIRREL_SPA",
                "Hello, hello! What a thrill this is, to find such warm comfort amidst the den of beasts.<page>" +
                "This is a ferocious place no doubt. Supposedly, there's a village deep in the warren. Its inhabitants never bowed to the King’s rule."));
            angleint.addLanguageString(new language_string("Quirrel", "DREAM_LAKE",
                "To live an age, yet remember so little... Perhaps I should be thankful?<page>" +
                "What good are somber memories that leave us unhappy?..."));
            angleint.addLanguageString(new language_string("Quirrel", "QUIRREL_EPILOGUE_A",
                "Again we meet, my strong friend. Here at last, I feel at peace.<page>" +
                "Twice I've seen this world and though my service may have stripped the first experience from me, I'm thankful I could witness its beauty again.<page>" +
                "Hallownest is a vast and wondrous thing, but with as many wonders as it holds, I've seen none quite so intriguing as you.<page>" +
                "Ha. My flattery returns only silent stoicism. I like that.<page>" +
                "I like that very much."));
            angleint.addLanguageString(new language_string("Quirrel", "QUIRREL_EPILOGUE_B", "...unbelievable..."));
            angleint.addLanguageString(new language_string("Quirrel", "QUIRREL_MANTIS_REPEAT",
                "My own route takes me towards that great city. If you search out the smith, it may not be long until our paths cross again."));
            angleint.addLanguageString(new language_string("Stag", "STAG_RUINS2",
                "Little one, we stand in the King's Station! Named of course for the King of Hallownest, he who demanded the building of the stagways and stations.<page>" +
                "The King never rode the stagways himself, but I've heard he was a glorious bug to behold, bright and radiant in visage, so much so it hurt to look at him. Not unlike you in that respect."));
            angleint.addLanguageString(new language_string("Stag", "STAG_TRAM",
                "That pass you hold! Is it not for that ghastly machine, the tram? I hope you're not thinking of riding on that grotesque contraption.<page>" +
                "The thought the King could build something like that to replace us. Foolish...Very foolish."));
            angleint.addLanguageString(new language_string("Enemy Dreams", "HOLLOW_KNIGHT_1",
                "...Stifle the light..."));
            angleint.addLanguageString(new language_string("Enemy Dreams", "HOLLOW_KNIGHT_2",
                "...Shall blaze... eternal...?"));
            angleint.addLanguageString(new language_string("Enemy Dreams", "HOLLOW_KNIGHT_3", "...For the King..."));
            angleint.addLanguageString(new language_string("Enemy Dreams", "HOLLOW_KNIGHT_4", "...Too radiant..."));
            angleint.addLanguageString(new language_string("Enemy Dreams", "HOLLOW_KNIGHT_5", "...Too dark..."));
            angleint.addLanguageString(new language_string("Enemy Dreams", "HOLLOW_KNIGHT_6", "...Too fiery..."));
            angleint.addLanguageString(new language_string("Enemy Dreams", "GENERIC_3", "...empty light..."));
            angleint.addLanguageString(new language_string("Enemy Dreams", "GENERIC_5", "...bright...hot..."));
            angleint.addLanguageString(new language_string("Enemy Dreams", "BEE_HATCHLING_2", "...hive aflame..."));
            angleint.addLanguageString(new language_string("Enemy Dreams", "HORNET_K_EDGE_2", "How is it so strong?"));
            angleint.addLanguageString(new language_string("Enemy Dreams", "BEE_2", "...Stop flames..."));
            angleint.addLanguageString(
                new language_string("Enemy Dreams", "MANTIS_PASSIVE_2", "...Outsider...safe?..."));
            angleint.addLanguageString(new language_string("Enemy Dreams", "MANTIS_PASSIVE_3", "...strong...hot..."));
            angleint.addLanguageString(new language_string("Enemy Dreams", "MANTIS_PASSIVE_1", "...bright warrior..."));
            angleint.addLanguageString(new language_string("Enemy Dreams", "GRIMM_1",
                "So radiant... So hot... incredible..."));
            angleint.addLanguageString(new language_string("Enemy Dreams", "GRIMM_2",
                "I would not want to be its enemy..."));
            angleint.addLanguageString(new language_string("Enemy Dreams", "GRIMM_3", "And on it burns..."));
            angleint.addLanguageString(new language_string("Enemy Dreams", "MOSS_FAT_1",
                "So bright...Yet something is wrong..."));
            angleint.addLanguageString(new language_string("Enemy Dreams", "MOSS_FAT_2",
                "...Light...Why do you torment us?"));
            angleint.addLanguageString(new language_string("Enemy Dreams", "MOSS_FAT_3", "Protect our prophet..."));
            angleint.addLanguageString(new language_string("Enemy Dreams", "DUNG_DEF_1",
                "...For the honour of the world!.."));
            angleint.addLanguageString(new language_string("Enemy Dreams", "MEGAFATBEE_1",
                "...Protect...At...All…Cost..."));
            angleint.addLanguageString(new language_string("Enemy Dreams", "MEGAFATBEE_2",
                "...Sisters...Mother...Colony..."));
            angleint.addLanguageString(new language_string("Enemy Dreams", "MANTISLORD_2", "...So strong..."));
            angleint.addLanguageString(new language_string("Enemy Dreams", "MANTISLORD_3", "...So hot..."));
            angleint.addLanguageString(new language_string("Enemy Dreams", "NIGHTMARE_GRIMM_1", "Perfectly empty..."));
            angleint.addLanguageString(new language_string("Enemy Dreams", "NIGHTMARE_GRIMM_2", "Perfectly fiery..."));
            angleint.addLanguageString(new language_string("Enemy Dreams", "NIGHTMARE_GRIMM_3",
                "Perfectly radiant..."));
            angleint.addLanguageString(new language_string("Enemy Dreams", "MOSS_KNIGHT_1",
                "...Dark light, in our leaves..."));
            angleint.addLanguageString(new language_string("Enemy Dreams", "MOSS_KNIGHT_2",
                "...Kill, for light...For Unn’s return..."));
            angleint.addLanguageString(new language_string("Enemy Dreams", "MOSS_KNIGHT_3",
                "...Protect...For the light’s return..."));
            angleint.addLanguageString(new language_string("Enemy Dreams", "MAGE_3",
                "...Their voices...cries of pain..."));
            angleint.addLanguageString(new language_string("Enemy Dreams", "MAGE_2", "...What have I done…?"));
            angleint.addLanguageString(new language_string("Enemy Dreams", "GRIMM_FLAMEBEARER_2",
                "Redwing. How great to see you…!"));
            angleint.addLanguageString(new language_string("Enemy Dreams", "GRIMM_FLAMEBEARER_1",
                "You’re so radiant. My flame dim by comparison..."));
            angleint.addLanguageString(new language_string("Enemy Dreams", "ZOMBIE_2", "...Your heat...Get away..."));
            angleint.addLanguageString(new language_string("Enemy Dreams", "ZOMBIE_3", "...I want to dream..."));
            angleint.addLanguageString(new language_string("Enemy Dreams", "ZOMBIE_4", "...Where am I…?"));
            angleint.addLanguageString(new language_string("Enemy Dreams", "ZOMBIE_5", "I cannot escape...wake up..."));
            angleint.addLanguageString(new language_string("Enemy Dreams", "ZOMBIE_6", "You’re fire...It hurts me..."));
            angleint.addLanguageString(new language_string("Enemy Dreams", "ZOMBIE_7", "I’m not dead..."));
            angleint.addLanguageString(new language_string("Enemy Dreams", "ZOMBIE_8", "The light...false..."));
            angleint.addLanguageString(new language_string("Enemy Dreams", "GRIMM_FLAMEBEARER_3",
                "As beautiful as on stage..."));
            angleint.addLanguageString(new language_string("Enemy Dreams", "MAGELORD_D_2",
                "Did their souls bring me closer to death?"));
            angleint.addLanguageString(new language_string("Enemy Dreams", "RADIANCE_5", "...I CANNOT DIE..."));
            angleint.addLanguageString(new language_string("Enemy Dreams", "RADIANCE_6",
                "...LIGHT AND VOID CANNOT MIX..."));
            angleint.addLanguageString(new language_string("Enemy Dreams", "RADIANCE_1", "...WHAT BEING ARE YOU..."));
            angleint.addLanguageString(new language_string("Enemy Dreams", "RADIANCE_2", "...I SHALL ESCAPE..."));
            angleint.addLanguageString(new language_string("Enemy Dreams", "RADIANCE_3", "...HOW CAN YOU EXIST..."));
            angleint.addLanguageString(new language_string("Enemy Dreams", "RADIANCE_4",
                "...THE NIGHTMARE FLAME WILL EXTINGUISH..."));
            angleint.addLanguageString(new language_string("Enemy Dreams", "WHITE_DEF_5",
                "Hallownest... Have faith in me!"));
            angleint.addLanguageString(new language_string("Enemy Dreams", "MANTIS_YOUNG_2",
                "...Kill...Prove strength..."));
            angleint.addLanguageString(new language_string("Enemy Dreams", "MANTIS_YOUNG_3", "...It needs fear..."));
            angleint.addLanguageString(new language_string("Enemy Dreams", "MANTIS_1", "...Intruder..."));
            angleint.addLanguageString(new language_string("Enemy Dreams", "MANTIS_2", "...Fight...Forever..."));
            angleint.addLanguageString(new language_string("Enemy Dreams", "MANTIS_3", "Resist...plague..."));
            angleint.addLanguageString(new language_string("Prompts", "NAILSMITH_UPGRADE_4",
                "Give three Pale Ore and Geo to strengthen your flame?"));
            angleint.addLanguageString(new language_string("Prompts", "NAILSMITH_UPGRADE_3",
                "Give two Pale Ore and Geo to strengthen your flame?"));
            angleint.addLanguageString(new language_string("Prompts", "NAILSMITH_UPGRADE_2",
                "Give Pale Ore and Geo to strengthen your flame?"));
            angleint.addLanguageString(new language_string("Prompts", "NAILSMITH_UPGRADE_1",
                "Pay Geo to strengthen your flame?"));
            angleint.addLanguageString(new language_string("Prompts", "GET_SHADOWDASH_1",
                "to dash in any direction, cloaked in shadow."));
            angleint.addLanguageString(new language_string("Prompts", "GET_SHADOWDASH_2",
                "Use the cloak to attack diagonally through enemies and their attacks without taking damage."));
            angleint.addLanguageString(new language_string("Prompts", "GET_FIREBALL2_1", "to unleash an oily spirit."));
            angleint.addLanguageString(new language_string("Prompts", "GET_QUAKE2_1",
                "while holding DOWN to strike the earth with a burst of fire and strength."));
            angleint.addLanguageString(new language_string("Prompts", "GET_QUAKE_1",
                "while holding DOWN to strike the earth with a burst of fire and strength."));
            angleint.addLanguageString(new language_string("Prompts", "GET_FIREBALL_1",
                "to unleash the flaming spirit."));
            angleint.addLanguageString(new language_string("Prompts", "GET_DASH_1", "to dash."));
            angleint.addLanguageString(new language_string("Prompts", "GET_DASH_2",
                "Use the cloak to dash quickly into the air."));
            angleint.addLanguageString(new language_string("Titles", "FINAL_BOSS_SUPER", "Corrupted"));
            angleint.addLanguageString(new language_string("UI", "CHARM_DESC_16",
                "Contains a forbidden spell that enhances the weapons of shadows.<br><br>When dashing, the bearer's body will travel much farther and faster."));
            angleint.addLanguageString(new language_string("UI", "CHARM_DESC_28",
                "Reveals the form of Unn within the bearer.<br><br>While focusing SOUL, the bearer will take on a new shape and can move freely to cheese bosses."));
            angleint.addLanguageString(new language_string("UI", "INV_NAME_NAIL4", "Coiled Firenail"));
            angleint.addLanguageString(new language_string("UI", "INV_NAME_NAIL5", "Pure Firenail"));
            angleint.addLanguageString(new language_string("UI", "CHARM_DESC_31",
                "Bears the likeness of an eccentric bug known only as 'The Dashmaster'.<br><br>The bearer will be able to dash twice in the air. Perfect for those who want to move where they shouldn’t be able to."));
            angleint.addLanguageString(new language_string("UI", "CHARM_DESC_34",
                "Naturally formed within a crystal over a long period. Draws in SOUL from the surrounding air.<br><br>The bearer will focus SOUL at a slower rate, but the healing effect will double and your fiery pillar will work below full health."));
            angleint.addLanguageString(new language_string("UI", "INV_NAME_NAIL1", "Old Firenail"));
            angleint.addLanguageString(new language_string("UI", "INV_NAME_NAIL2", "Sharpened Firenail"));
            angleint.addLanguageString(new language_string("UI", "INV_NAME_NAIL3", "Channeled Firenail"));
            angleint.addLanguageString(new language_string("UI", "CHARM_DESC_40_N",
                "Token commemorating the failure of Redwing.<br><br>Contains a song of protection that may defend the bearer from damage... but not shame."));
            angleint.addLanguageString(new language_string("UI", "INV_DESC_SPELL_FOCUS",
                "Focus collected SOUL to repair your shell and heal or cast a fiery pillar.<br><br>Strike enemies to gather SOUL."));
            angleint.addLanguageString(new language_string("UI", "CHARM_DESC_36_A",
                "Fragment of a radiant charm. It hums with energy but cannot be equipped."));
            angleint.addLanguageString(new language_string("UI", "INV_DESC_SPELL_SCREAM2",
                "Blast foes with screaming SOUL and Flame.<br><br>The Wraiths requires SOUL to be conjured. Strike enemies to gather SOUL."));
            angleint.addLanguageString(new language_string("UI", "INV_DESC_SPELL_QUAKE2",
                "Strike the ground with a concentrated force of SOUL and Flame. This force can destroy foes or break through fragile structures.<br><br>The force requires SOUL to be conjured. Strike enemies to gather SOUL."));
            angleint.addLanguageString(new language_string("UI", "CHARM_NAME_36_B", "Radiant Heart"));
            angleint.addLanguageString(new language_string("UI", "CHARM_NAME_36_C", "Lightbringer"));
            angleint.addLanguageString(new language_string("UI", "CHARM_DESC_36_C",
                "An emptiness from within seals and condenses the radiance inside. " +
                "Can be opened to briefly unleash a flaming light as radiant as a thousand suns." +
                "<br><br>This charm cannot, and probably should not be unequipped."));
            angleint.addLanguageString(new language_string("UI", "CHARM_DESC_36_B",
                "Radiant charm containing all the untainted light of Hallownest. " +
                "It emits an endless luminous stream of energy the bearer can see and feel.<br><br>" +
                "Opens the way to a birthplace."));
            angleint.addLanguageString(new language_string("Journal", "DESC_INFECTED_KNIGHT",
                "Dying beast, sustained by infected parasites."));
            angleint.addLanguageString(new language_string("Journal", "NOTE_ZOM_HORNHEAD",
                "These bugs have an arrogant air about them. Overly proud of their long horns! I enjoy snapping them off."));
            angleint.addLanguageString(new language_string("Journal", "NOTE_MOSS_WALKER",
                "I used to think these things were merely ambling plants. Now that I see them as actually living creatures, I began to kill them on sight. This is the nature of the Hunt!"));
            angleint.addLanguageString(new language_string("Journal", "DESC_ZOM_MINER",
                "A bug drawn to the Crystal Peak for its precious crystal. Its claw-pick now doubles as a fierce weapon."));
            angleint.addLanguageString(new language_string("Journal", "DESC_TRAITOR_LORD",
                "Deposed Lord of the Mantis tribe. Embraced the light and turned against his sisters."));
            angleint.addLanguageString(new language_string("Journal", "NOTE_MUMMY",
                "Inside these beings is a bright light that pierces any darkness. I peered inside that light once and saw... something within it shining back. Something terrible."));
            angleint.addLanguageString(new language_string("Journal", "NOTE_SENTRY_FAT",
                "Surprisingly quick-witted for prey. It will try to catch you if you leap over it and can chase you if you flee. Enjoyable to hunt, but beware them in packs."));
            angleint.addLanguageString(new language_string("Journal", "DESC_FINAL_BOSS", "The light, corrupted."));
            angleint.addLanguageString(new language_string("Journal", "NOTE_HOLLOW_KNIGHT",
                "The old King of Hallownest... he must have been desperate to save his crumbling throne. The sacrifices he imposed on others... all for an infection."));
            angleint.addLanguageString(new language_string("Journal", "DESC_GHOST_MARKOTH",
                "Lingering dream of a fallen warrior. Only member of his slaughtered tribe to take up a weapon."));
            angleint.addLanguageString(new language_string("Journal", "NOTE_MENDERBUG",
                "Collateral damage can be justified, if the gain outweighs the cost. How much do you think a completed journal is worth?"));
            angleint.addLanguageString(new language_string("Journal", "NAME_FINAL_BOSS", "Radiance?"));
            angleint.addLanguageString(new language_string("Journal", "NOTE_ZOM_BURSTING",
                "The bugs of Hallownest were twisted out of shape by that corrupting sickness. First they fell into deep slumber, then they awoke with broken minds, and then their bodies started to deform..."));
            angleint.addLanguageString(new language_string("Journal", "NOTE_FINAL_BOSS",
                "The plague, the infection, the madness that haunts the corpses of Hallownest... All because a battle neither side won? An endless dance between darkness and light that mortal minds will never understand."));
            angleint.addLanguageString(new language_string("Journal", "DESC_GRIMM",
                "Master of the Grimm Troupe. The greatest being in the world."));
            angleint.addLanguageString(new language_string("Journal", "DESC_SENTRY_1",
                "A corrupted Hallownest Sentry. Still retains some memory of its former task."));
            angleint.addLanguageString(new language_string("Journal", "DESC_ZOM_BEAM_MINER",
                "A bug who lived amongst the crystals. Seemingly controlled by a strange force, it attacks by firing beams of light from its crystallised arms."));
            angleint.addLanguageString(new language_string("Journal", "NOTE_ZOM_ROYAL_2",
                "The fearful, cowardly nature of these Hallownest bugs persists even after their corruption! It almost feels shameful to chase and cut them down."));
            angleint.addLanguageString(new language_string("Journal", "DESC_ZOM_BARGER",
                "A corrupted bug, controlled by a strange force. It aggressively attacks anyone who threatens the infection."));
            angleint.addLanguageString(new language_string("Journal", "DESC_GREAT_SHIELD_ZOMBIE",
                "A corrupted Great Sentry, the most elite of the city's guards. Wields a greatnail and shell. Its powerful attacks cause heavy damage."));
            angleint.addLanguageString(new language_string("Journal", "DESC_ZOM_LEAPER",
                "A corrupted bug, controlled by a strange force. It will instinctively leap at anyone who threatens the infection to attack them."));
            angleint.addLanguageString(new language_string("Journal", "DESC_ZOM_HORNHEAD",
                "A corrupted bug, controlled by a strange force. Uses its horn to attack anyone who threatens the infection."));
            angleint.addLanguageString(new language_string("Journal", "DESC_ZOM_SHIELD",
                "A corrupted bug, controlled by a strange force. Wields a nail and shell."));
            angleint.addLanguageString(new language_string("Journal", "DESC_ZOM_RUNNER",
                "A corrupted bug, controlled by a strange force. Wanders the roads where it once lived."));
            angleint.addLanguageString(new language_string("Journal", "DESC_HOLLOW_KNIGHT",
                "Fully grown Vessel. Its void forms one half of the infection."));
            angleint.addLanguageString(new language_string("Journal", "DESC_ZOM_GUARD",
                "A once great Hallownest Crossguard, controlled by a strange force. Instinct still drives it to guard its post against intruders."));
            angleint.addLanguageString(new language_string("Journal", "NOTE_ZOM_BARGER",
                "A thick orange mist fills these corrupted bugs. It has a sugary taste to it. I find it foul. After you kill these creatures, I suggest you do not eat them."));
            angleint.addLanguageString(new language_string("Journal", "NOTE_ZOM_RUNNER", "These \"civilised\" bugs of Hallownest were weak when uninfected and equally weak today. " +
                                                                                         "Send them back into the dirt they were born in!"));
            angleint.addLanguageString(new language_string("Journal", "DESC_NIGHTMARE_GRIMM",
                "Beautiful spectre of scarlet flame."));
            angleint.addLanguageString(new language_string("CP2", "GRIMM_DEFEAT_2",
                "Look, Redwing! How our child has grown, nourished and strengthened by the heat of our passionate dance!<page>" +
                "The two of you will feature in many tragedies and triumphs together, I'm sure.<page>" +
                "And so our great Ritual nears its end. Continue to harvest the flame and let us complete another cycle!<page>" +
                "Our scarlet eyes will watch you keenly... friend."));
            angleint.addLanguageString(new language_string("CP2", "ISELDA_GRIMM",
                "Have you seen them? The travellers who set up camp outside of town?<page>" +
                "With all due respect, they have this... sinister feeling to them. It reminds me of you in some ways. You didn’t bring them here, did you?<page>" +
                "Regardless, I've told Cornifer he's not to speak to them. His heart and his mind are a little too open sometimes."));
            angleint.addLanguageString(new language_string("CP2", "BRUMM_DREAM_DEEPNEST",
                "We must complete...EXTINGUISH...for Grimm, for the ritual."));
            angleint.addLanguageString(new language_string("CP2", "GRIMM_RITUAL_COMPLETE",
                "You’ve seen this done before but I can refresh you on the process.<page>" +
                "My kin have collected some of the flames of this kingdom. Seek them, claim it, and return it to me. Together, marvels shall be achieved.<page>" +
                "You should have the honor of working with my child, you discovered this land after all. It shall guide you to the flame and gather within itself that burning essence.<page>" +
                "Like you, the child plays key role in this task. Only with it by your side will the flame, and my kin, reveal themselves to you."));
            angleint.addLanguageString(new language_string("CP2", "ISELDA_NYMM",
                "How peculiar, a stylish musician made his way to town. I asked him about you and he seems to dislike you.<page>" +
                "He told me you betrayed some guy named Grimm... Now I don’t trust random strangers but this is quite the peculiar accusation indeed."));
            angleint.addLanguageString(new language_string("CP2", "IGOR_MEET",
                "Redwing, you called us?<page>" +
                "Speak to Master."));
            angleint.addLanguageString(new language_string("CP2", "QUEEN_GRIMMCHILD_FULL",
                "Your companion's eyes burn with a flame not unlike your own... Success then for the scarlet heart, and for you I suppose as well.<page>" +
                "As beautiful as it is, you and I both know this land could never bear another king. The crown of Hallownest is fruitless, and its scepter barren."));
            angleint.addLanguageString(new language_string("CP2", "BRETTA_DIARY_LEAVE",
                "The Maiden's Quest<br>Her Grey Prince diminished and her Red Saviour revealed as beast, the Maiden at last understood the truth.<page>" +
                "Her life's companion would not appear, for they could not appear to a maiden sat idle. She must instead seek them out, must find her love, and free them of their solitude.<page>" +
                "And thus her own journey began, out into dangerous lands, shielded by her love awaiting, guided by her love to be.<page>" +
                "With every step, the maiden could feel it, their fated meeting, coming ever closer."));
            angleint.addLanguageString(new language_string("CP2", "GRIMM_DREAM",
                "Masterful! Although I should have expected nothing less.<page>" +
                "Even though I taught it, its talents still impress me every time."));
            angleint.addLanguageString(new language_string("CP2", "NYMM_REPEAT",
                "Are you enjoying my music? It's as upbeat a tune as I know, but I must admit, even it falls a little on the sadder side.<page>" +
                "I felt bad for this town... They trusted you and you ruined it. This town needs some happiness to counterbalance your failure."));
            angleint.addLanguageString(new language_string("CP2", "NYMM_DREAM", "...Do you feel like a hero yet?"));
            angleint.addLanguageString(new language_string("CP2", "NYMM_FINAL",
                "Brownwing, I should probably mention I heard a terrible scream down below. Was it pain, or rage? Perhaps another being you betrayed. If so I hope you’re happy with what you’ve done.<page>" +
                "I’d urge you to head down and let it tear you in half."));
            angleint.addLanguageString(new language_string("CP2", "NYMM_MEET",
                "Oh, it’s you! What a sad town you have created through your actions, brownwing. You’d best stay here because the whole world outside this land hates you.<page>" +
                "The old bug over there was very welcoming, and he seemed quite fond of you, surprisingly. This place is quite melancholy, what with the wind, and the darkness, and the sense of decline... I am here to liven up the place after your failure.<page>" +
                "With my music the whole town feels brighter. And good thing too for your light has seemingly faded.<page>" +
                "I was told to give you a small gift to commemorate your failing. It will occasionally protect you from taking damage but is mostly useless.<page>" +
                "Take it, as a white elephant from me to you, and never talk to me again."));
            angleint.addLanguageString(new language_string("CP2", "ELDERBUG_NYMM",
                "Ah ha! Good riddance! That creepy carnival has vanished and town's returned to its former self, yet something seems lost.<page>" +
                "Sure, we've gained a new addition to the square! But he talks very poorly of you. I wonder what you could have done to deserve that.<page>" +
                "At any rate, he's a rather good musician, and this town's spent far too long listening only to the wind. Thanks to him, we've gained a new tune and some new company!"));
            angleint.addLanguageString(new language_string("CP2", "BRUMM_LANTERN",
                "So you followed me here, to where the Ritual began. Leave at once if you value your reputation and the troupe.<page>" +
                "I cannot control myself much longer... I have an urge to destroy it all that brings me here. Leave now before I change my mind and murder the child.<page>" +
                "Finish the ritual... play your role... save me!"));
            angleint.addLanguageString(new language_string("CP2", "GRIMM_DEFEAT_1",
                "Bravo, my friend. Isn’t it lovely to hear the crowd adore you once again? They've not seen such a show in a long time."));
            angleint.addLanguageString(new language_string("CP2", "DIVINE_MEET",
                "Aaaaaaaaahhhhhhhh!<page>" +
                "Did you call us? You called us, and we came. We came!<page>" +
                "It’s so great to see you once again, Redwing!<page>" +
                "But why have you brought us here?<page>" +
                "Anyway, we came, and I can smell something. Something deep below us. I want it... I want it!"));
            angleint.addLanguageString(new language_string("CP2", "GRIMM_BATTLE_3",
                "Dance with me, my friend. The crowd awaits. Let’s show them your power once again!"));
            angleint.addLanguageString(new language_string("CP2", "GRIMM_ACCEPT",
                "As the lantern flared your role was cast, our compact written in scarlet fire.<page>" +
                "Eager to carry the child for the first time? But first, some illumination is required."));
            angleint.addLanguageString(new language_string("CP2", "QUEEN_GRIMMCHILD",
                "Ahh, that creature beside. It is odd to me, but you two share a... similarity? It is a distant link, one words would strain to convey. It seems like a friend of sorts.<page>" +
                "For it to cling to you now... You've been consumed in the Ritual of that scarlet clan. But you’ve worked for them before it seems...<page>" +
                "In what poor moment they descend upon our ruin. Aid their propagation, if you so choose, but do not renege on the larger task this kingdom implores."));
            angleint.addLanguageString(new language_string("CP2", "DIVINE_DREAM",
                "Shadow, fire, and light. What a beautiful dance..."));
            angleint.addLanguageString(new language_string("CP2", "NYMM_BRUMM_CHARM",
                "Ahh. Just marvelous! So now the traitor comes back wearing my joke charm unironically. It’s as bad at humor as rituals.<page>" +
                "To be honest, I'm not even sure where I came upon it, but staring at its design, it’s probably something I picked up while intoxicated.<page>" +
                "The specifics elude me, but a strange sense remains, fear, but also... corruption? Like a strange dream I could not escape.<page>" +
                "I've no need for it in any case, and I hope its uselessness gets you killed."));
            angleint.addLanguageString(new language_string("CP2", "BRUMM_DEEPNEST_NF_1",
                "Mrmm. You came.<page>" +
                "You have gathered the flame and so the time for the ritual's completion is upon us. All you must do now is return to the Master...<page>" +
                "How beautiful it must be, to be like the notes in an old, old song. You and me. Mrmm.<page>" +
                "But why would I come down to the darkest, furthest reaches of the world. I sense a strange, corrupting force throughout this dying kingdom. I fled to get as far as I could and clear my head but it doesn’t seem to help.<page>" +
                "Mrmm. I’d wish to help with the song that never ends, but I fear for myself.<page>" +
                "Strange thoughts enter my head. Desires to burn it all down and cast the master out. I do not think I can go on with these rituals.<page>" +
                "The darkness does not seem to stop the infection’s cruel grasp, so I shall try going to the Howling Cliffs next..."));
            angleint.addLanguageString(new language_string("CP2", "BRUMM_DEEPNEST_1",
                "Mrmm. You came.<page>" +
                "You have gathered the flame and so the time for the ritual's completion is upon us. All you must do now is return to the Master...<page>" +
                "How beautiful it must be, to be like the notes in an old, old song. You and me. Mrmm."));
            angleint.addLanguageString(new language_string("CP2", "BRUMM_DEEPNEST_2",
                "Endless, repeating songs of sacrifice, of servitude. For the Ritual. For the troupe. For the Master.<page>" +
                "...Such thoughts have infected my mind much to my dismay."));
            angleint.addLanguageString(new language_string("CP2", "BRUMM_DEEPNEST_3",
                "So we serve... Thus it has ever been. Yes?<page>" +
                "Take the flame then, it is why you came here."));
            angleint.addLanguageString(new language_string("CP2", "BRUMM_DEEPNEST_4",
                "It is done... and yet...<page>" +
                "But why would I come down to the darkest, furthest reaches of the world. I sense a strange, corrupting force throughout this dying kingdom. I fled to get as far as I could and clear my head.<page>" +
                "Mrmm. I’d wish to help with the song that never ends, but I fear for myself.<page>" +
                "Strange thoughts enter my head. Desires to burn it all down and cast the master out. I do not think I can go on with these rituals.<page>" +
                "The darkness does not seem to stop the infection’s cruel grasp, so I shall try going to the Howling Cliffs next...<page>" +
                "Do not seek me out. Return to the Master and complete the Ritual... I will bear you no hatred."));
            angleint.addLanguageString(new language_string("CP2", "BRUMM_DEEPNEST_REPEAT",
                "Do not try to follow me. I will be where it began...<page>" +
                "Please, complete the ritual, for me, for Master, and for yourself, Redwing."));
            angleint.addLanguageString(new language_string("CP2", "GRIMM_MEET2",
                "Good to see you once again, Redwing. As you must know by now, I am Grimm, master of this troupe.<page>" +
                "The lantern has been lit, and your summons heeded. Quite the interesting stage you found. This kingdom fallowed by worm and root, perfect earth upon which our Ritual shall take place.<page>" +
                "As for you, my friend. Your own part is far from over."));
            angleint.addLanguageString(new language_string("CP2", "BRUMM_LANTERN_DREAM",
                "Master... Let us complete the ritual once more, for the nightmare!"));
            angleint.addLanguageString(new language_string("CP2", "JIJI_GRIMM",
                "Hmm. A fierce host has settled nearby. Did you summon them? I find their smell... slightly sweet yet toxic... unsettling.<page>" +
                "Theirs is a scent of distant technologies, unfamiliar even to me. But one which resembles you.<page>" +
                "Perhaps it is their size but they look so... gaudy and intimidating. They favour projection over truth, shrouding themselves in forms dreamed. No wonder the strength of your small statue must impress them so.<page>" +
                "You needn’t disguise yourself to impress them with your mastery of flame."));
            angleint.addLanguageString(new language_string("CP2", "ELDERBUG_GRIMM",
                "Ahh! Look there! Something strange yet familiar has suddenly appeared!<page>" +
                "It reminds me a bit of you. I will do my best to try to ignore it."));
            angleint.addLanguageString(new language_string("CP2", "ELDERBUG_TROUPE_LEFT",
                "Ah ha! And just like that the carnival is gone, and with it, the town has returned to its quietness.<page>" +
                "Nothing against them but it's quite enough to deal with just the occasional traveller. Whole structures appearing out of nowhere are far more than one old bug should have to face."));
            angleint.addLanguageString(new language_string("Lore Tablets", "GREEN_TABLET_03",
                "The greater mind once dreamed of sentient leaf and cast these caverns so.<br>In every bush and every vine an intelligence cast by the mind of Unn."));
            angleint.addLanguageString(new language_string("Lore Tablets", "KING_FINAL_WORDS",
                "<page>" +
                "...What madness have I unleashed...<page>" +
                "...I never meant for this to happen..."));
            angleint.addLanguageString(new language_string("Lore Tablets", "RUIN_TAB_01",
                "In the city of Hallownest...<br>No one ever enters.<br>And no one ever leaves."));
            angleint.addLanguageString(new language_string("Lore Tablets", "RUIN_TAB_02",
                "Missing monarch, we need you now. A madness spreads and nothing can stop it. You must return to us."));
            angleint.addLanguageString(new language_string("Lore Tablets", "FUNG_SHROOM_DREAM",
                "Pale Wyrm...Cannot foresee anything after all?"));
            angleint.addLanguageString(new language_string("Lore Tablets", "HIGHER_BEING_CORPSE",
                "...No king... No mind... Happiness..."));
            angleint.addLanguageString(new language_string("Lore Tablets", "ABYSS_TUT_TAB_01",
                "Higher beings, these words are for you alone.<br><br>Our perfect Vessel has ascended.<br>Let us forget the refuse and regret of its creation.<br>And remember the prosperity it brought."));
            angleint.addLanguageString(new language_string("Lore Tablets", "ARCHIVE_01",
                "-THIR-YOLK-ABA-ABSENCE-OUTER-SHELL-O-GATE-CONTAIN<br>LIGHT-EM-VESSEL-EM-EGG-EM-SEAL-THIR-WITHDRAW<br>EXTERIOR-O-SEAL-WITHIN-DREAMER-INTWIXT-ATWIXT<br>CONTAIN-LIGHT-THIR-DREAM-CH-ABA-REBELLION-CONTAIN-"));
            angleint.addLanguageString(new language_string("Lore Tablets", "ARCHIVE_02",
                "-KINGLIGHT-EM-GROWTH-INKIND-FLOW-ALLPOWER-ENFIELD<br>OLDLIGHT-EM-ESSENCE-EM-DREAM-EM-ENEMY<br>O-BRIGHTNESS-ABA-KINGLIGHT-CH-OLDLIGHT-THEMKIN-O-CH<br>WILLFLOW-ABA-DREAM-UNCONTAIN-THIR-ENDKINGDOM-ENDLIFE-"));
            angleint.addLanguageString(new language_string("Credits List", "CREDITS_THANKS_NAME",
                "Ludum Dare<br>Warren Fenn and Rohan Fraser<br>Ben Gibson and Makoto Koji<br>Trent Kusters and Morgan Jaffit<br>Rod Jago and Tim Mcburnie<br>John Millard and Jason Pamment<br>Zara Pellen and Victoria Roberts<br>Lilly Sim and Nicola Stark<br>Sharyn Stone and Steven Sun<br>Matt Trobbiani and Dan Treble<br>Peter Yong and Jenni Vigaud<br>Chris Wright<br><br>Redwing Thanks<br>FoldingPapers<br>Gradow<br>The Knightmere<br>Grimm Crime Sindicate<br>The Redwing Server<br><br>And you, for supporting modding"));
            angleint.addLanguageString(new language_string("Credits List", "CREDITS_TESTERS_NAME_COL_01",
                "Hari Dimitriou<br>Tyler Bartley<br>Lili Carlyle<br>Joshua Clark<br>" +
                "Andrew Cook<br>Shannon Cross<br>Cale \"Embraced\" Firgren<br>Rohan Fraser<br>Yusuf Bham"));
            angleint.addLanguageString(new language_string("Credits List", "CREDITS_TESTERS_NAME_COL_02",
                "Ben Newhouse<br>Pirate-Rob<br>Unborn_Pho3nix<br>Kyle Pulver<br>" +
                "Greg Smith<br>Luke Souris<br>Matthew White<br>Verulean<br>Ayako Hibino"));
            
            log("Completed adding lore and stuff.");
        }
        
        
        // only used once blackmoth actually gets lore
        private static void setupBlackmothLoreEN()
        {
            log("Replacing blackmoth lore with redwing lore.");
        }
        
        private static void log(string str)
        {
            Modding.Logger.Log("[Redwing] " + str);
        }
    }
}