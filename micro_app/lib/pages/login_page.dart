import 'package:flutter/material.dart';
import '../services/api_client.dart';
import '../services/auth_storage.dart';
import 'items_page.dart';

class LoginPage extends StatefulWidget {
  const LoginPage({super.key});

  @override
  State<LoginPage> createState() => _LoginPageState();
}

class _LoginPageState extends State<LoginPage> {
  final _formKey = GlobalKey<FormState>();
  final _emailController = TextEditingController();
  final _passwordController = TextEditingController();
  final _api = ApiClient();
  final _storage = AuthStorage();
  bool _loading = false;
  String? _error;
  List<Credential> _saved = [];

  @override
  void initState() {
    super.initState();
    _loadSaved();
  }

  Future<void> _loadSaved() async {
    final list = await _storage.getCredentials();
    if (!mounted) return;
    setState(() => _saved = list);
  }

  @override
  void dispose() {
    _emailController.dispose();
    _passwordController.dispose();
    super.dispose();
  }

  Future<void> _login() async {
    if (!_formKey.currentState!.validate()) return;
    setState(() {
      _loading = true;
      _error = null;
    });
    final email = _emailController.text.trim();
    final password = _passwordController.text;
    try {
      final token = await _api.login(
        email: email,
        password: password,
      );
      await _storage.saveToken(token);
      await _storage.upsertCredential(email: email, password: password);
      if (!mounted) return;
      Navigator.of(context).pushReplacement(
        MaterialPageRoute(builder: (_) => const ItemsPage()),
      );
    } catch (e) {
      setState(() {
        _error = e.toString();
      });
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(_error!)),
      );
    } finally {
      if (mounted) {
        setState(() {
          _loading = false;
        });
        // Refresh saved list to reflect MRU ordering after successful login
        _loadSaved();
      }
    }
  }

  void _useSaved(Credential c) {
    if (_loading) return;
    _emailController.text = c.email;
    _passwordController.text = c.password;
    _login();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Login')),
      body: Center(
        child: ConstrainedBox(
          constraints: const BoxConstraints(maxWidth: 400),
          child: Padding(
            padding: const EdgeInsets.all(16.0),
            child: Form(
              key: _formKey,
              child: SingleChildScrollView(
                child: Column(
                  mainAxisSize: MainAxisSize.min,
                  crossAxisAlignment: CrossAxisAlignment.stretch,
                  children: [
                    TextFormField(
                      controller: _emailController,
                      decoration: const InputDecoration(labelText: 'email'),
                      textInputAction: TextInputAction.next,
                      validator: (v) => (v == null || v.trim().isEmpty) ? 'Enter email' : null,
                    ),
                    const SizedBox(height: 12),
                    TextFormField(
                      controller: _passwordController,
                      decoration: const InputDecoration(labelText: 'Password'),
                      obscureText: true,
                      validator: (v) => (v == null || v.isEmpty) ? 'Enter password' : null,
                      onFieldSubmitted: (_) => _login(),
                    ),
                    const SizedBox(height: 20),
                    SizedBox(
                      width: double.infinity,
                      child: ElevatedButton.icon(
                        onPressed: _loading ? null : _login,
                        icon: _loading
                            ? const SizedBox(
                                width: 16,
                                height: 16,
                                child: CircularProgressIndicator(strokeWidth: 2),
                              )
                            : const Icon(Icons.login),
                        label: const Text('Login'),
                      ),
                    ),
                    if (_error != null) ...[
                      const SizedBox(height: 12),
                      Text(_error!, style: const TextStyle(color: Colors.red)),
                    ],
                    if (_saved.isNotEmpty) ...[
                      const SizedBox(height: 24),
                      const Text('Saved logins', style: TextStyle(fontWeight: FontWeight.bold)),
                      const SizedBox(height: 8),
                      ListView.separated(
                        shrinkWrap: true,
                        physics: const NeverScrollableScrollPhysics(),
                        itemCount: _saved.length,
                        separatorBuilder: (_, __) => const Divider(height: 1),
                        itemBuilder: (context, index) {
                          final c = _saved[index];
                          return ListTile(
                            leading: const Icon(Icons.account_circle),
                            title: Text(c.email),
                            subtitle: Text('••••••••'),
                            trailing: const Icon(Icons.chevron_right),
                            onTap: () => _useSaved(c),
                          );
                        },
                      ),
                    ],
                  ],
                ),
              ),
            ),
          ),
        ),
      ),
    );
  }
}
