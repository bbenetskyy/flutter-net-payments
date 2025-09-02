import '../../models/item.dart';
import '../item_repository.dart';
import '../users_repository.dart';

/// Adapter that exposes Users as generic Items for reuse of the Items UI.
class RestUsersAsItemsRepository implements ItemRepository {
  RestUsersAsItemsRepository(this._users);
  final UsersRepository _users;

  @override
  Future<List<Item>> fetchItems() async {
    final data = await _users.listUsers();
    final list = _unwrapList(data);
    return list.map(_mapUser).toList();
  }

  @override
  Future<Item> fetchItem(String id) async {
    final data = await _users.getUserById(id);
    return _mapUser(Map<String, dynamic>.from(data as Map));
  }

  @override
  Future<Item> upsertItem(Item item) async => item; // not supported here

  @override
  Future<void> deleteItem(String id) async {
    // Users delete not exposed here; noop
  }

  List<Map<String, dynamic>> _unwrapList(dynamic data) {
    if (data is List) return data.cast<Map<String, dynamic>>();
    if (data is Map && data['items'] is List) {
      return (data['items'] as List).cast<Map<String, dynamic>>();
    }
    if (data is Map && data['data'] is List) {
      return (data['data'] as List).cast<Map<String, dynamic>>();
    }
    return const [];
  }

  Item _mapUser(Map<String, dynamic> j) {
    String _s(dynamic v) => v?.toString() ?? '';
    final id = _s(j['id'] ?? j['userId']);
    final email = _s(j['email']);
    final name = _s(j['displayName'] ?? j['name'] ?? email.split('@').first);
    final status = _s(j['status'] ?? j['state']);
    final role = _s(j['role'] ?? j['roles']);

    return Item(
      id: id.isEmpty ? DateTime.now().millisecondsSinceEpoch.toString() : id,
      title: name.isEmpty ? email : name,
      subtitle: email,
      description: status.isEmpty
          ? (role.isEmpty ? '' : 'Role: $role')
          : 'Status: $status${role.isEmpty ? '' : ' · Role: $role'}',
    );
  }
}
