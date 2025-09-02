import 'dart:collection';

import '../models/item.dart';
import 'item_repository.dart';

/// Simple in-memory implementation of ItemRepository for demo/prototyping.
/// Keeps data only in memory (lost on page reload).
class InMemoryItemRepository implements ItemRepository {
  InMemoryItemRepository({String? seedKey}) : _seedKey = seedKey ?? 'default';

  final String _seedKey;
  static final Map<String, List<Item>> _store = HashMap();

  List<Item> get _items => _store[_seedKey] ??= <Item>[];

  @override
  Future<List<Item>> fetchItems() async {
    // Return a copy to avoid external mutation
    return List<Item>.unmodifiable(_items);
  }

  @override
  Future<Item> fetchItem(String id) async {
    final item = _items.firstWhere((e) => e.id == id);
    return item;
  }

  @override
  Future<Item> upsertItem(Item item) async {
    final idx = _items.indexWhere((e) => e.id == item.id);
    if (idx == -1) {
      _items.insert(0, item);
    } else {
      _items[idx] = item;
    }
    return item;
  }

  @override
  Future<void> deleteItem(String id) async {
    _items.removeWhere((e) => e.id == id);
  }
}
