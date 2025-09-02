import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

class ThemeCubit extends Cubit<ThemeMode> {
  ThemeCubit() : super(ThemeMode.system);
  void toggle() => emit(state == ThemeMode.dark ? ThemeMode.light : ThemeMode.dark);
  void set(ThemeMode mode) => emit(mode);
}
