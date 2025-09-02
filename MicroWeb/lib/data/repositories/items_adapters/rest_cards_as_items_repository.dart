import '../../models/item.dart';
import '../cards_repository.dart';
import '../item_repository.dart';

/// Adapter that exposes Cards as generic Items for reuse of the Items UI.
class RestCardsAsItemsRepository implements ItemRepository {
  RestCardsAsItemsRepository(this._cards);
  final CardsRepository _cards;

  @override
  Future<List<Item>> fetchItems() async {
    final data = await _cards.listCards();
    final list = _unwrapList(data);
    return list.map(_mapCard).toList();
  }

  @override
  Future<Item> fetchItem(String id) async {
    final data = await _cards.getCardById(id);
    return _mapCard(Map<String, dynamic>.from(data as Map));
  }

  @override
  Future<Item> upsertItem(Item item) async => item; // not supported

  @override
  Future<void> deleteItem(String id) async {
    // Optional: call delete if exposed
    try {
      await _cards.deleteCard(id);
    } catch (_) {
      // ignore for demo if not supported by backend
    }
  }

  List<Map<String, dynamic>> _unwrapList(dynamic data) {
    if (data is List) return data.cast<Map<String, dynamic>>();
    if (data is Map && data['items'] is List) {
      return (data['items'] as List).cast<Map<String, dynamic>>();
    }
    return const [];
  }

  Item _mapCard(Map<String, dynamic> j) {
    String _s(dynamic v) => v?.toString() ?? '';
    final id = _s(j['id']);
    final name = _s(j['name'] ?? j['label'] ?? j['alias'] ?? 'Card');
    final type = _s(j['type']);
    final status = _s(j['status']);
    final panMasked = _s(j['panMasked'] ?? j['maskedPan'] ?? j['last4']);
    final printed = j['printed']?.toString();

    return Item(
      id: id.isEmpty ? DateTime.now().millisecondsSinceEpoch.toString() : id,
      title: name.isEmpty ? 'Card $panMasked' : name,
      subtitle: panMasked.isEmpty ? type : '$type · $panMasked',
      description: status.isEmpty ? '' : 'Status: $status${printed == null ? '' : ' · Printed: $printed'}',
    );
  }
}
