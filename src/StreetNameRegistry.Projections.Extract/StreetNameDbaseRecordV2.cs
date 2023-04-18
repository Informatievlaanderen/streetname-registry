namespace StreetNameRegistry.Projections.Extract
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public sealed class StreetNameDbaseRecordV2 : DbaseRecord
    {
        public static readonly StreetNameDbaseSchemaV2 Schema = new StreetNameDbaseSchemaV2();

        public DbaseCharacter id { get; }
        public DbaseInt32 straatnmid { get; }
        public DbaseCharacter creatieid { get; }
        public DbaseCharacter versieid { get; }
        public DbaseCharacter gemeenteid { get; }
        public DbaseCharacter straatnm { get; }
        public DbaseCharacter homoniemtv { get; }
        public DbaseCharacter status { get; }

        public StreetNameDbaseRecordV2()
        {
            id = new DbaseCharacter(Schema.id);
            straatnmid = new DbaseInt32(Schema.straatnmid);
            creatieid = new DbaseCharacter(Schema.creatieid);
            versieid = new DbaseCharacter(Schema.versieid);
            gemeenteid = new DbaseCharacter(Schema.gemeenteid);
            straatnm = new DbaseCharacter(Schema.straatnm);
            homoniemtv = new DbaseCharacter(Schema.homoniemtv);
            status = new DbaseCharacter(Schema.status);

            Values = new DbaseFieldValue[]
            {
                id,
                straatnmid,
                creatieid,
                versieid,
                gemeenteid,
                straatnm,
                homoniemtv,
                status
            };
        }
    }
}
