import 'package:flutter/foundation.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:go_router/go_router.dart';
import '../../data/models/requests/update_user_request.dart';
import '../../data/models/responses/role_response.dart';
import '../../data/models/responses/user_response.dart';
import '../../data/models/user_permissions.dart';
import '../../data/repositories/users_repository.dart';

class UserDetailPage extends StatefulWidget {
  const UserDetailPage({super.key, required this.user, required this.roles});
  final UserResponse user;
  final List<RoleResponse> roles;

  @override
  State<UserDetailPage> createState() => _UserDetailPageState();
}

class _UserDetailPageState extends State<UserDetailPage> {
  late UserResponse _user;

  @override
  void initState() {
    super.initState();
    _user = widget.user;
  }

  @override
  Widget build(BuildContext context) {
    final title = (_user.displayName ?? _user.email ?? 'NAME MISSING');
    return Scaffold(
      body: Padding(
        padding: const EdgeInsets.all(24.0),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              children: [
                IconButton(onPressed: () => context.pop(), icon: const Icon(Icons.arrow_back)),
                const SizedBox(width: 8),
                const Icon(Icons.person, size: 28),
                const SizedBox(width: 8),
                Expanded(
                  child: Text(title, style: const TextStyle(fontSize: 24, fontWeight: FontWeight.bold)),
                ),
                const SizedBox(width: 8),
                FilledButton.icon(
                  onPressed: () => _showEditDialog(context),
                  icon: const Icon(Icons.edit),
                  label: const Text('Edit'),
                ),
              ],
            ),
            const SizedBox(height: 12),
            if ((_user.email ?? '').isNotEmpty) Text(_user.email!, style: const TextStyle(color: Colors.grey)),
            const SizedBox(height: 12),
            if ((_user.role ?? '').isNotEmpty) Text('Role: ${_user.role}'),
            if ((_user.createdAt ?? '').isNotEmpty) Text('Created: ${_user.createdAt}'),
          ],
        ),
      ),
    );
  }

  Future<void> _showEditDialog(BuildContext context) async {
    final nameController = TextEditingController(text: _user.displayName ?? '');
    final emailController = TextEditingController(text: _user.email ?? '');
    DateTime? selectedDob; // We only have dobHash in response; allow setting a new DOB
    RoleResponse? selectedRole;
    final formKey = GlobalKey<FormState>();
    bool saving = false;

    await showDialog(
      context: context,
      builder: (ctx) {
        return StatefulBuilder(
          builder: (ctx, setState) {
            selectedRole = widget.roles.firstWhere(
              (r) => r.name.toLowerCase() == (_user.role ?? '').toLowerCase(),
              orElse: () => widget.roles.isNotEmpty
                  ? widget.roles.first
                  : RoleResponse(
                      id: '',
                      name: '',
                      permissions: const <UserPermissions>{},
                      createdAt: DateTime.now(),
                      usersCount: 0,
                    ),
            );

            return AlertDialog(
              title: const Text('Edit user'),
              content: SizedBox(
                width: 420,
                child: Form(
                  key: formKey,
                  child: Column(
                    mainAxisSize: MainAxisSize.min,
                    children: [
                      TextFormField(
                        controller: nameController,
                        decoration: const InputDecoration(labelText: 'Name'),
                      ),
                      const SizedBox(height: 12),
                      TextFormField(
                        controller: emailController,
                        decoration: const InputDecoration(labelText: 'Email'),
                        enabled: false, // Backend update does not support email change
                      ),
                      const SizedBox(height: 12),
                      Row(
                        children: [
                          Expanded(
                            child: InputDecorator(
                              decoration: const InputDecoration(labelText: 'Date of birth'),
                              child: InkWell(
                                onTap: () async {
                                  final now = DateTime.now();
                                  final first = DateTime(now.year - 100, 1, 1);
                                  final last = DateTime(now.year, now.month, now.day);
                                  final picked = await showDatePicker(
                                    context: ctx,
                                    initialDate: selectedDob ?? DateTime(now.year - 18, now.month, now.day),
                                    firstDate: first,
                                    lastDate: last,
                                  );
                                  if (picked != null) setState(() => selectedDob = picked);
                                },
                                child: Padding(
                                  padding: const EdgeInsets.symmetric(vertical: 12.0),
                                  child: Text(
                                    selectedDob == null
                                        ? 'Not set'
                                        : '${selectedDob!.year.toString().padLeft(4, '0')}-${selectedDob!.month.toString().padLeft(2, '0')}-${selectedDob!.day.toString().padLeft(2, '0')}',
                                    style: TextStyle(color: Theme.of(context).colorScheme.onSurfaceVariant),
                                  ),
                                ),
                              ),
                            ),
                          ),
                        ],
                      ),
                      const SizedBox(height: 12),
                      DropdownButtonFormField<RoleResponse>(
                        value: selectedRole,
                        decoration: const InputDecoration(labelText: 'Role'),
                        items: widget.roles
                            .map((r) => DropdownMenuItem<RoleResponse>(value: r, child: Text(r.name)))
                            .toList(),
                        onChanged: (v) => setState(() => selectedRole = v),
                      ),
                    ],
                  ),
                ),
              ),
              actions: [
                TextButton(onPressed: saving ? null : () => Navigator.of(ctx).pop(), child: const Text('Cancel')),
                FilledButton(
                  onPressed: saving
                      ? null
                      : () async {
                          setState(() => saving = true);
                          try {
                            final req = UpdateUserRequest(
                              displayName: nameController.text.trim().isEmpty ? null : nameController.text.trim(),
                              roleId: selectedRole?.id,
                              dateOfBirth: selectedDob,
                            );
                            if (_user.id != null) {
                              //todo await usersRepo.adminUpdateUser(_user.id!, req);
                            }
                            setState(() => saving = false);
                            if (mounted) {
                              setState(() {
                                _user = UserResponse(
                                  id: _user.id,
                                  email: _user.email,
                                  displayName: nameController.text.trim().isEmpty
                                      ? _user.displayName
                                      : nameController.text.trim(),
                                  role: selectedRole?.name ?? _user.role,
                                  effectivePermissions: _user.effectivePermissions,
                                  dobHash: _user.dobHash, // cannot recalc without backend feedback
                                  verificationStatus: _user.verificationStatus,
                                  createdAt: _user.createdAt,
                                );
                              });
                              Navigator.of(ctx).pop();
                              ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('User updated')));
                            }
                          } catch (e) {
                            setState(() => saving = false);
                            ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text('Failed: $e')));
                          }
                        },
                  child: saving
                      ? const SizedBox(width: 16, height: 16, child: CircularProgressIndicator(strokeWidth: 2))
                      : const Text('Save'),
                ),
              ],
            );
          },
        );
      },
    );
  }
}
