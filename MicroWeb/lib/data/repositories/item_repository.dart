import '../models/item.dart';

abstract class ItemRepository {
  Future<List<Item>> fetchItems();
  Future<Item> fetchItem(String id);
  Future<Item> upsertItem(Item item);
  Future<void> deleteItem(String id);
}
