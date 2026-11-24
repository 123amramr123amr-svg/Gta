cmake_minimum_required(VERSION 3.22)
project(driving LANGUAGES CXX)
add_library(driving SHARED driving.cpp)
target_compile_features(driving PRIVATE cxx_std_17)
