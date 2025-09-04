import 'dart:convert';

import 'package:flutter_test/flutter_test.dart';
import 'package:shared_preferences/shared_preferences.dart';

import 'package:micro_app/services/auth_storage.dart';

void main() {
  group('AuthStorage saved credentials persistence', () {
    setUp(() async {
      // Start each test with clean prefs
      SharedPreferences.setMockInitialValues({});
    });

    test('upsertCredential persists and getCredentials returns MRU order', () async {
      final storage = AuthStorage();

      await storage.upsertCredential(username: 'alice', password: 'a1');
      await storage.upsertCredential(username: 'bob', password: 'b1');
      await storage.upsertCredential(username: 'alice', password: 'a2'); // moves to front with updated pass

      final list = await storage.getCredentials();
      expect(list.length, 2);
      expect(list[0].username, 'alice');
      expect(list[0].password, 'a2');
      expect(list[1].username, 'bob');
      expect(list[1].password, 'b1');
    });

    test('credentials remain available across app restarts (simulated)', () async {
      // Simulate that an earlier app run stored two credentials.
      // We encode them exactly as AuthStorage does: List<String> of JSON.
      final seed = <String, Object>{
        'saved_credentials_v1': [
          jsonEncode({'username': 'user1', 'password': 'p1'}),
          jsonEncode({'username': 'user2', 'password': 'p2'}),
        ],
      };
      SharedPreferences.setMockInitialValues(seed);

      // "New app launch": create a fresh AuthStorage and read.
      final storage = AuthStorage();
      final list = await storage.getCredentials();

      expect(list.length, 2);
      expect(list[0].username, 'user1');
      expect(list[0].password, 'p1');
      expect(list[1].username, 'user2');
      expect(list[1].password, 'p2');
    });
  });
}
