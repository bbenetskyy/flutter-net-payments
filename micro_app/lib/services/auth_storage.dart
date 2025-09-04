import 'dart:convert';
import 'package:shared_preferences/shared_preferences.dart';

class AuthStorage {
  static const _keyToken = 'auth_token';
  static const _keyCreds = 'saved_credentials_v1';
  static const int _maxCreds = 10;

  Future<void> saveToken(String token) async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.setString(_keyToken, token);
  }

  Future<String?> getToken() async {
    final prefs = await SharedPreferences.getInstance();
    return prefs.getString(_keyToken);
  }

  Future<void> clear() async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.remove(_keyToken);
  }

  // Saved Credentials API
  Future<List<Credential>> getCredentials() async {
    final prefs = await SharedPreferences.getInstance();
    final raw = prefs.getStringList(_keyCreds) ?? const [];
    final list = <Credential>[];
    for (final s in raw) {
      try {
        final m = jsonDecode(s) as Map<String, dynamic>;
        list.add(Credential.fromJson(m));
      } catch (_) {}
    }
    return list;
  }

  Future<void> upsertCredential({required String email, required String password}) async {
    final prefs = await SharedPreferences.getInstance();
    final current = await getCredentials();
    // Remove existing same-email entry to avoid duplicates; then insert at start (MRU)
    current.removeWhere((c) => c.email == email);
    current.insert(0, Credential(email: email, password: password));
    // Enforce max size
    if (current.length > _maxCreds) {
      current.removeRange(_maxCreds, current.length);
    }
    final encoded = current.map((c) => jsonEncode(c.toJson())).toList();
    await prefs.setStringList(_keyCreds, encoded);
  }
}

class Credential {
  final String email;
  final String password;
  Credential({required this.email, required this.password});

  Map<String, dynamic> toJson() => {
        'email': email,
        'password': password,
      };

  factory Credential.fromJson(Map<String, dynamic> json) =>
      Credential(email: json['email']?.toString() ?? '', password: json['password']?.toString() ?? '');
}
