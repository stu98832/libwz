namespace libwz.Tools
{
    /// <summary> </summary>
    public enum MapleTextFormat
    {
        /// <summary> </summary>
        UnknowFormat = 1,
        /// <summary> </summary>
        PlainText = 0,
        /// <summary> </summary>
        ParseFail = -1,

        //// TEXT FORMAT ////
        /// <summary> change font color to black </summary>
        ColorBlack = 'k',
        /// <summary> change font color to violet </summary>
        ColorViolet = 'd',
        /// <summary> change font color to red </summary>
        ColorRed = 'r',
        /// <summary> change font color to green </summary>
        ColorGreen = 'g',
        /// <summary> change font color to blue </summary>
        ColorBlue = 'b',
        /// <summary> set bold font</summary>
        FontBold = 'e',
        /// <summary> set regular font</summary>
        FontRegular = 'n',
        /// <summary> option tag </summary>
        OptionBegin = 'L',
        /// <summary> option end tag </summary>
        OptionEnd = 'l',
        /// <summary> display user name </summary>
        UserName = 'h',
        /// <summary> display item name </summary>
#warning TODO: check t & z
        ItemName = 't',
        /// <summary> display item name with information </summary>
        ItemNameWithInfo = 'z',
#warning TODO: check i & v
        /// <summary> display item icon </summary>
        ItemIcon = 'i',
        /// <summary> display item icon with information </summary>
        ItemIconWithInfo = 'v',
        /// <summary> display item count </summary>
        ItemCount = 'c',
        /// <summary> display skill name </summary>
        SkillName = 'q',
        /// <summary> display skill icon </summary>
        SkillIcon = 's',
        /// <summary> display quest name </summary>
        QuestName = 'y',
        /// <summary> display quest summary icon </summary>
        QuestSummaryIcon = 'W',
#warning TODO: check this
        /// <summary> display quest's state </summary>
        QuestState = 'u',
#warning TODO: check this
        /// <summary> display quest's value </summary>
        QuestValue = 'R',
        /// <summary> display quest's mob count </summary>
        QuestMobCount = 'a',
        /// <summary> display quest's mob name </summary>
        QuestMobName = 'M',
        /// <summary> display quest's bonus exp </summary>
        QuestBonusExp = 'x',
#warning TODO: check this
        /// <summary> display quest gauge </summary>
        QuestGauge = 'j',
        /// <summary> display quest's progress state </summary>
        QuestProgressState = 'u',
        /// <summary> display map name </summary>
        MapName = 'm',
        /// <summary> display mob name </summary>
        MobName = 'o',
        /// <summary> display npc name </summary>
        NpcName = 'p',
        /// <summary> display npc name without link </summary>
        NpcNameWithoutLink = '@',
        /// <summary> display progress bar </summary>
        ProgressBar = 'B',
        /// <summary> display canvas </summary>
        ShowCanvas = 'f',
        /// <summary> display canvas </summary>
        ShowCanvas2 = 'F',
    }
}
