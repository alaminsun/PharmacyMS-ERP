import { CatalogDefinition } from './models';

export const CATALOGS: CatalogDefinition[] = [
  { key: 'medicines', title: 'Medicines', endpoint: '/medicines', permission: 'Medicines', columns: ['code','name','strength','categoryName','genericName','sellingPrice','minimumStockLevel','isActive'], fields: [
    { key:'code',label:'Medicine code',required:true }, { key:'name',label:'Medicine name',required:true }, { key:'strength',label:'Strength' },
    { key:'categoryId',label:'Category',type:'select',options:'categories' }, { key:'genericId',label:'Generic',type:'select',options:'generics' },
    { key:'shelfId',label:'Shelf',type:'select',options:'shelves' }, { key:'supplierId',label:'Supplier',type:'select',options:'suppliers' },
    { key:'buyingPrice',label:'Buying price',type:'number',required:true,defaultValue:0 }, { key:'sellingPrice',label:'Selling price',type:'number',required:true,defaultValue:0 },
    { key:'minimumStockLevel',label:'Minimum stock level',type:'number',required:true,defaultValue:10 },
    { key:'isActive',label:'Active',type:'checkbox',defaultValue:true }
  ] },
  { key:'categories',title:'Categories',endpoint:'/categories',permission:'Categories',columns:['id','name','description'],fields:[
    {key:'name',label:'Name',required:true},{key:'description',label:'Description'}] },
  { key:'generics',title:'Generics',endpoint:'/generics',permission:'Generics',columns:['id','code','name'],fields:[
    {key:'code',label:'Code',required:true},{key:'name',label:'Name',required:true}] },
  { key:'shelves',title:'Shelves',endpoint:'/shelves',permission:'Shelves',columns:['id','number','name'],fields:[
    {key:'number',label:'Shelf number',required:true},{key:'name',label:'Name',required:true}] },
  { key:'suppliers',title:'Suppliers',endpoint:'/suppliers',permission:'Suppliers',columns:['code','name','shortName','email','phone','address'],fields:[
    {key:'code',label:'Code',required:true},{key:'name',label:'Name',required:true},{key:'shortName',label:'Short name'},
    {key:'email',label:'Email',type:'email'},{key:'phone',label:'Phone'},{key:'address',label:'Address'},
    {key:'notes',label:'Notes',type:'textarea'}] },
  { key:'customers',title:'Customers',endpoint:'/customers',permission:'Customers',columns:['code','name','email','phone','address'],fields:[
    {key:'code',label:'Code',required:true},{key:'name',label:'Name',required:true},{key:'email',label:'Email',type:'email'},
    {key:'phone',label:'Phone'},{key:'address',label:'Address'},{key:'notes',label:'Notes',type:'textarea'}] }
];
