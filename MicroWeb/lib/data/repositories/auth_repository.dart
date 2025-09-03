import '../models/user.dart';

abstract class AuthRepository {
  Future<User?> currentUser();
  Future<User> signIn({required String email, required String password});
  Future<bool> signUp({required String email, required String password, String? displayName});
  Future<void> signOut();
}
