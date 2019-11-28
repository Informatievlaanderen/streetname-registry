namespace StreetNameRegistry.Projections.Extract
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class StreetNameDbaseSchema : DbaseSchema
    {
        public DbaseField id => Fields[0];
        public DbaseField straatnmid => Fields[1];
        public DbaseField versieid => Fields[2];
        public DbaseField gemeenteid => Fields[3];
        public DbaseField straatnm => Fields[4];
        public DbaseField homoniemtv => Fields[5];
        public DbaseField status => Fields[6];

        public StreetNameDbaseSchema() => Fields = new[]
        {
            DbaseField.CreateStringField(new DbaseFieldName(nameof(id)), new DbaseFieldLength(50)),
            DbaseField.CreateInt32Field(new DbaseFieldName(nameof(straatnmid)), new DbaseFieldLength(10)),
            DbaseField.CreateStringField(new DbaseFieldName(nameof(versieid)), new DbaseFieldLength(25)),
            DbaseField.CreateStringField(new DbaseFieldName(nameof(gemeenteid)), new DbaseFieldLength(5)),
            DbaseField.CreateStringField(new DbaseFieldName(nameof(straatnm)), new DbaseFieldLength(80)),
            DbaseField.CreateStringField(new DbaseFieldName(nameof(homoniemtv)), new DbaseFieldLength(5)),
            DbaseField.CreateStringField(new DbaseFieldName(nameof(status)), new DbaseFieldLength(50))
        };
    }
}
