namespace Script.Map {

    public class Campaign_04 : Campaign {

        public Field targetField;

        protected override Mission[] GetMissions() {
            return new Mission[]{
                new OccupyField(targetField, "North West tower")
            };
        }

        protected override void OnStartCampaign() {
            SetGold(0, 800);
            SetGold(1, 300);
        }

    }

}