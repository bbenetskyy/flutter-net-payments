import 'package:flutter/material.dart';
import '../models/item.dart';
import '../services/api_client.dart';
import '../services/auth_storage.dart';
import 'login_page.dart';

enum ItemType { users, cards, payments }

class ItemsPage extends StatefulWidget {
  const ItemsPage({super.key});

  @override
  State<ItemsPage> createState() => _ItemsPageState();
}

class _ItemsPageState extends State<ItemsPage> {
  bool _accept = true;
  final _api = ApiClient();
  final _storage = AuthStorage();
  String? _token;
  Future<List<Item>>? _future;
  ItemType _selectedType = ItemType.users;

  @override
  void initState() {
    super.initState();
    _init();
  }

  Future<void> _init() async {
    final token = await _storage.getToken();
    if (!mounted) return;
    if (token == null || token.isEmpty) {
      _goToLogin();
      return;
    }
    setState(() {
      _token = token;
      _future = _loadItems();
    });
  }

  String _endpointFor(ItemType type) {
    switch (type) {
      case ItemType.users:
        return 'users';
      case ItemType.cards:
        return 'cards';
      case ItemType.payments:
        return 'payments';
    }
  }

  Future<List<Item>> _loadItems() async {
    if (_token == null) return [];
    final list = await _api.getItemsAt(token: _token!, endpoint: _endpointFor(_selectedType));
    return list.map((e) => Item.fromJson(e)).toList();
  }

  Future<void> _logout() async {
    await _storage.clear();
    if (!mounted) return;
    _goToLogin();
  }

  void _goToLogin() {
    Navigator.of(context).pushAndRemoveUntil(MaterialPageRoute(builder: (_) => const LoginPage()), (_) => false);
  }

  void _onTypeSelected(int index) {
    final newType = ItemType.values[index];
    if (newType == _selectedType) return;
    setState(() {
      _selectedType = newType;
      _future = _loadItems();
    });
  }

  @override
  Widget build(BuildContext context) {
    final isSelected = ItemType.values.map((t) => t == _selectedType).toList();
    final labels = const ['Users', 'Cards', 'Payments'];

    return Scaffold(
      appBar: AppBar(
        title: const Text('Items'),
        actions: [IconButton(onPressed: _logout, icon: const Icon(Icons.logout), tooltip: 'Logout')],
      ),
      body: _future == null
          ? const Center(child: CircularProgressIndicator())
          : Column(
              children: [
                Padding(
                  padding: const EdgeInsets.fromLTRB(12, 12, 12, 0),
                  child: ToggleButtons(
                    isSelected: isSelected,
                    onPressed: _onTypeSelected,
                    borderRadius: const BorderRadius.all(Radius.circular(8)),
                    selectedBorderColor: Theme.of(context).colorScheme.primary,
                    borderColor: Theme.of(context).dividerColor,
                    selectedColor: Theme.of(context).colorScheme.onPrimary,
                    color: Theme.of(context).colorScheme.primary,
                    fillColor: Theme.of(context).colorScheme.primary.withOpacity(0.85),
                    constraints: const BoxConstraints(minHeight: 40, minWidth: 100),
                    children: [
                      for (var i = 0; i < labels.length; i++)
                        Padding(padding: const EdgeInsets.symmetric(horizontal: 8.0), child: Text(labels[i])),
                    ],
                  ),
                ),
                const SizedBox(height: 8),
                Padding(
                  padding: const EdgeInsets.symmetric(horizontal: 12.0),
                  child: Row(
                    children: [
                      const Icon(Icons.toggle_on),
                      const SizedBox(width: 8),
                      const Text('Accept decisions'),
                      const Spacer(),
                      Switch(value: _accept, onChanged: (v) => setState(() => _accept = v)),
                    ],
                  ),
                ),
                const SizedBox(height: 8),
                Expanded(
                  child: RefreshIndicator(
                    onRefresh: () async {
                      setState(() {
                        _future = _loadItems();
                      });
                      await _future;
                    },
                    child: FutureBuilder<List<Item>>(
                      future: _future,
                      builder: (context, snapshot) {
                        if (snapshot.connectionState == ConnectionState.waiting) {
                          return const Center(child: CircularProgressIndicator());
                        }
                        if (snapshot.hasError) {
                          return ListView(
                            physics: const AlwaysScrollableScrollPhysics(),
                            children: [
                              Padding(
                                padding: const EdgeInsets.all(16.0),
                                child: Text(
                                  'Failed to load items:\n${snapshot.error}',
                                  style: const TextStyle(color: Colors.red),
                                ),
                              ),
                            ],
                          );
                        }
                        final items = snapshot.data ?? [];
                        if (items.isEmpty) {
                          return ListView(
                            physics: const AlwaysScrollableScrollPhysics(),
                            children: const [
                              SizedBox(height: 100),
                              Center(child: Text('No items available')),
                            ],
                          );
                        }
                        return ListView.separated(
                          physics: const AlwaysScrollableScrollPhysics(),
                          itemCount: items.length,
                          separatorBuilder: (_, __) => const Divider(height: 1),
                          itemBuilder: (context, index) {
                            final item = items[index];
                            String _fmtDate(DateTime? dt) {
                              if (dt == null) return '';
                              // yyyy-MM-dd HH:mm
                              final y = dt.year.toString().padLeft(4, '0');
                              final m = dt.month.toString().padLeft(2, '0');
                              final d = dt.day.toString().padLeft(2, '0');
                              final hh = dt.hour.toString().padLeft(2, '0');
                              final mm = dt.minute.toString().padLeft(2, '0');
                              return '$y-$m-$d $hh:$mm';
                            }

                            final details = <String>[];
                            if (item.id != null) details.add('ID: ${item.id}');
                            if ((item.code ?? '').isNotEmpty) details.add('Code: ${item.code}');
                            if (item.statusLabel.isNotEmpty) details.add('Status: ${item.statusLabel}');
                            if (item.actionLabel.isNotEmpty) details.add('Action: ${item.actionLabel}');
                            if ((item.targetId ?? '').isNotEmpty) details.add('Target: ${item.targetId}');
                            if ((item.assigneeUserId ?? '').isNotEmpty) details.add('Assignee: ${item.assigneeUserId}');
                            final createdStr = _fmtDate(item.createdAt);
                            if (createdStr.isNotEmpty) details.add('Created: $createdStr');
                            final decidedStr = _fmtDate(item.decidedAt);
                            if (decidedStr.isNotEmpty) details.add('Decided: $decidedStr');
                            if ((item.createdBy ?? '').isNotEmpty) details.add('By: ${item.createdBy}');
                            final subtitleText = details.isNotEmpty ? details.join('\n') : null;

                            Color? tileColor;
                            switch (item.status) {
                              case VerificationStatus.Pending:
                                tileColor = Colors.amber.shade50;
                                break;
                              case VerificationStatus.Rejected:
                                tileColor = Colors.red.shade50;
                                break;
                              case VerificationStatus.Completed:
                                tileColor = Colors.green.shade50;
                                break;
                              default:
                                tileColor = null;
                            }

                            return ListTile(
                              title: Text(item.name),
                              subtitle: subtitleText != null ? Text(subtitleText) : null,
                              leading: const Icon(Icons.list_alt),
                              isThreeLine: details.length >= 2,
                              tileColor: tileColor,
                              onTap: () async {
                                if (_token == null) return;
                                if (item.status != VerificationStatus.Pending) {
                                  if (!context.mounted) return;
                                  ScaffoldMessenger.of(
                                    context,
                                  ).showSnackBar(const SnackBar(content: Text('No action needed')));
                                  return;
                                }
                                try {
                                  await _api.postVerificationDecision(
                                    token: _token!,
                                    endpoint: _endpointFor(_selectedType),
                                    id: item.id,
                                    code: item.code!,
                                    isUsers: _selectedType == ItemType.users,
                                    targetId: item.targetId,
                                    accept: _accept,
                                  );
                                  if (!context.mounted) return;
                                  ScaffoldMessenger.of(
                                    context,
                                  ).showSnackBar(const SnackBar(content: Text('Decision submitted')));
                                  setState(() {
                                    _future = _loadItems();
                                  });
                                } catch (e) {
                                  if (!context.mounted) return;
                                  ScaffoldMessenger.of(
                                    context,
                                  ).showSnackBar(SnackBar(content: Text('Failed to submit decision: $e')));
                                }
                              },
                            );
                          },
                        );
                      },
                    ),
                  ),
                ),
              ],
            ),
    );
  }
}
