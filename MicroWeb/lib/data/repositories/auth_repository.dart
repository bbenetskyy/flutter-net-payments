import '../models/user.dart';

abstract class AuthRepository {
  Future<User?> currentUser();
  Future<User> signIn({required String email, required String password});
  Future<User> signUp({required String email, required String password, String? displayName});
  Future<void> signOut();
}
