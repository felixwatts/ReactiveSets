using System;

namespace ReactiveSets
{
    public class Mapper<TIdIn, TPayloadIn, TIdOut, TPayloadOut> : SetToSet<TIdIn, TPayloadIn, TIdOut, TPayloadOut>
    {
        private readonly Func<TIdIn, TIdOut> _idMapping;
        private readonly Func<TIdIn, TPayloadIn, TPayloadOut> _payloadMapping;
        private readonly bool _disposeOnDelete;

        public Mapper(ISet<TIdIn, TPayloadIn> source, Func<TIdIn, TIdOut> idMapping, Func<TIdIn, TPayloadIn, TPayloadOut> payloadMapping, bool disposeOnDelete = true)
            : base(source)
        {
            _idMapping = idMapping;
            _payloadMapping = payloadMapping;
            _disposeOnDelete = disposeOnDelete;
        }

        protected override void OnSetItem(TIdIn id, TPayloadIn payload)
        {
            var idOut = _idMapping(id);
            var payloadOut = _payloadMapping(id, payload);

            _content.SetItem(idOut, payloadOut);
        }

        protected override void OnDeleteItem(TIdIn id)
        {
            var idOut = _idMapping(id);  

            var oldItem = _content[idOut];    

            _content.DeleteItem(idOut);

            if(_disposeOnDelete)
            {                
                (oldItem as IDisposable)?.Dispose();
            }    
        }
    } 
}