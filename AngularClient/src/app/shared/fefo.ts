import type { SaleLineInput, StockBatch } from '../core/models';
/** First-expiring-first-out, excluding expired/zero batches. No stock mutation. */
export function allocateFefo(batches:StockBatch[], medicineId:number, quantity:number):SaleLineInput[] {
  if(!Number.isSafeInteger(quantity)||quantity<1)throw new Error('Quantity must be a positive whole number.');
  const eligible=batches.filter(b=>b.medicineId===medicineId&&!b.isExpired&&b.quantityOnHand>0)
    .sort((a,b)=>a.expiryDate.localeCompare(b.expiryDate)||a.stockBatchId-b.stockBatchId);
  let needed=quantity;
  const allocation:SaleLineInput[]=[];
  for(const b of eligible){if(!needed)break;const take=Math.min(needed,b.quantityOnHand);
    allocation.push({stockBatchId:b.stockBatchId,quantity:take}); needed-=take;}
  if(needed>0)throw new Error(`Not enough stock for medicine #${medicineId}. Missing ${needed} unit(s).`);
  return allocation;
}
