namespace ReactiveSets
{
    public struct Delta<TId, TPayload>
    {
        public DeltaType Type { get; private set; }
        public TId Id { get; private set; }
        public TPayload Payload { get; private set; }

        public static Delta<TId, TPayload> Clear = new Delta<TId, TPayload>(DeltaType.Clear, default(TId), default(TPayload));
        public static Delta<TId, TPayload> BeginBulkUpdate = new Delta<TId, TPayload>(DeltaType.BeginBulkUpdate, default(TId), default(TPayload));
        public static Delta<TId, TPayload> EndBulkUpdate = new Delta<TId, TPayload>(DeltaType.EndBulkUpdate, default(TId), default(TPayload));
        public static Delta<TId, TPayload> SetItem(TId id, TPayload payload) { return new Delta<TId, TPayload>(DeltaType.SetItem, id, payload ); }
        public static Delta<TId, TPayload> DeleteItem(TId id) { return new Delta<TId, TPayload>(DeltaType.DeleteItem, id, default(TPayload) ); }

        private Delta(DeltaType type, TId id, TPayload payload)
        {
            Type = type;
            Id = id;
            Payload = payload;
        }
    }
}