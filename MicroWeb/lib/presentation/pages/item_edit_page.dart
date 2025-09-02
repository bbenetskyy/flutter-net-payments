import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:go_router/go_router.dart';
import '../../data/models/item.dart';
import '../../logic/items/items_bloc.dart';
import '../../core/utils/validators.dart';

class ItemEditPage extends StatefulWidget {
  const ItemEditPage({super.key, required this.id});
  final String id;

  @override
  State<ItemEditPage> createState() => _ItemEditPageState();
}

class _ItemEditPageState extends State<ItemEditPage> {
  final _formKey = GlobalKey<FormState>();
  late final TextEditingController _title;
  late final TextEditingController _subtitle;
  late final TextEditingController _description;

  @override
  void initState() {
    super.initState();
    final bloc = context.read<ItemsBloc>();
    final existing = bloc.state.items.where((e) => e.id == widget.id).cast<Item?>().firstOrNull;
    _title = TextEditingController(text: existing?.title ?? '');
    _subtitle = TextEditingController(text: existing?.subtitle ?? '');
    _description = TextEditingController(text: existing?.description ?? '');
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Edit item')),
      body: Padding(
        padding: const EdgeInsets.all(16),
        // Above line keeps analyzer calm without importing extra constraints helpers.
        child: Center(
          child: ConstrainedBox(
            constraints: const BoxConstraints(maxWidth: 640),
            child: Form(
              key: _formKey,
              child: Column(
                children: [
                  TextFormField(
                    controller: _title,
                    decoration: const InputDecoration(labelText: 'Title'),
                    validator: (v) => Validators.required(v),
                  ),
                  const SizedBox(height: 12),
                  TextFormField(
                    controller: _subtitle,
                    decoration: const InputDecoration(labelText: 'Subtitle'),
                  ),
                  const SizedBox(height: 12),
                  TextFormField(
                    controller: _description,
                    decoration: const InputDecoration(labelText: 'Description'),
                    maxLines: 5,
                  ),
                  const SizedBox(height: 16),
                  Row(
                    children: [
                      ElevatedButton.icon(
                        onPressed: () {
                          if (_formKey.currentState!.validate()) {
                            final item = Item(
                              id: widget.id,
                              title: _title.text,
                              subtitle: _subtitle.text,
                              description: _description.text,
                            );
                            context.read<ItemsBloc>().add(ItemSaved(item));
                            context.go('/items/${widget.id}');
                          }
                        },
                        icon: const Icon(Icons.save),
                        label: const Text('Save'),
                      ),
                      const SizedBox(width: 12),
                      OutlinedButton.icon(
                        onPressed: () => context.pop(),
                        icon: const Icon(Icons.close),
                        label: const Text('Cancel'),
                      ),
                    ],
                  ),
                ],
              ),
            ),
          ),
        ),
      ),
    );
  }
}
