﻿using Poorsat;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal class Suudoku
{
  private readonly int N;
  private readonly int blockN;
  private List<List<string>> ConditionsList;

  public Suudoku(int N)
  {
    this.N = N;
    blockN = (int)Math.Sqrt(N);
  }

  /// <summary>
  /// <para>与えられた数独のリストから数独を解く</para>
  /// <para>数独のリストは以下のように与えるものとする</para>
  /// <para>   List{List{string}} p = new List{List{string}}(){</para>
  /// <para>     new List{string}(){" "," ","1"," "},</para>
  /// <para>     new List{string}(){" ","3"," ","4"},</para>
  /// <para>     new List{string}(){"3"," ","4"," "},</para>
  /// <para>     new List{string}(){" ","2"," "," "},</para>
  ///     };
  /// </summary>
  /// <param name="sudoku">数独のリスト</param>
  /// <param name="result">結果を格納する配列</param>
  /// <returns></returns>
  public bool Solve(List<List<string>> sudoku, out int[,] result)
  {
    /**************************************************
     * 各マスにN個の条件を付与
     **************************************************/

    List<List<List<string>>> masCond = new List<List<List<string>>>();

    //各列について
    for(int k = 0; k < N; ++k)
    {
      masCond.Add(new List<List<string>>());

      //各行について
      for(int j = 0; j < N; ++j)
      {
        masCond[k].Add(new List<string>());

        //各マスN個の条件
        for(int i = 0; i < N; ++i)
          masCond[k][j].Add(k.ToString() + j.ToString() + (i + 1).ToString());
      }
    }

    //ShowConditional(masCond, "各マスの条件");

    /**************************************************
     * 各行に同じ数字が2度現れない条件を付与
     **************************************************/
    List<List<List<List<string>>>> rowCond = new List<List<List<List<string>>>>();

    //各行について
    for(int j = 0; j < N; ++j)
    {
      rowCond.Add(new List<List<List<string>>>());

      for(int i = 0; i < N; ++i)
      {
        rowCond[j].Add(new List<List<string>>());
        foreach(var c in Combinations(Enumerable.Range(0, N).ToList(), 2).Select((_v, _i) => new { _v, _i }))
        {
          rowCond[j][i].Add(new List<string>());
          foreach(var l in c._v.Select((_v, _i) => new { _v, _i }))
            rowCond[j][i][c._i].Add("-" + j.ToString() + l._v.ToString() + (i + 1).ToString());
        }
      }
    }

    //foreach(var r in rowCond)
    //  ShowConditional(r, "各行の条件");

    /**************************************************
     * 各列に同じ数字が2度現れない条件を付与
     **************************************************/
    List<List<List<List<string>>>> colCond = new List<List<List<List<string>>>>();

    //各行について
    for(int j = 0; j < N; ++j)
    {
      colCond.Add(new List<List<List<string>>>());
      for(int i = 0; i < N; ++i)
      {
        colCond[j].Add(new List<List<string>>());
        foreach(var c in Combinations(Enumerable.Range(0, N).ToList(), 2).Select((_v, _i) => new { _v, _i }))
        {
          colCond[j][i].Add(new List<string>());
          foreach(var l in c._v.Select((_v, _i) => new { _v, _i }))
            colCond[j][i][c._i].Add("-" + l._v.ToString() + j.ToString() + (i + 1).ToString());
        }
      }
    }

    //foreach(var r in colCond)
    //  ShowConditional(r, "各列の条件");

    /**************************************************
     * 各ブロックに同じ数字が2度現れない条件を付与
     **************************************************/
    List<List<string>> blockCond = new List<List<string>>();
    List<string> buf = new List<string>();

    for(int yBlock = 0; yBlock < N; yBlock += blockN)
      for(int xBlock = 0; xBlock < N; xBlock += blockN)
      {
        //o0~onまで
        for(int n = 0; n < blockN * blockN; ++n)
        {
          //o0ブロックcheck
          for(int j = 0; j < blockN; ++j)
            for(int i = 0; i < blockN; ++i)
              buf.Add("-" + j.ToString() + i.ToString() + (n + 1).ToString());
          foreach(var c in Combinations(buf, 2))
            blockCond.Add(c);
          buf = new List<string>();
        }
      }

    /**************************************************
     * 数独初期値から条件生成
     **************************************************/

    /**************************************************
     * Solverに渡すリストの作成
     **************************************************/
    ConditionsList = new List<List<string>>();

    foreach(var a in masCond)
      foreach(var b in a)
        ConditionsList.Add(b);
    foreach(var a in rowCond)
      foreach(var b in a)
        foreach(var c in b)
          ConditionsList.Add(c);
    foreach(var a in colCond)
      foreach(var b in a)
        foreach(var c in b)
          ConditionsList.Add(c);
    foreach(var a in blockCond)
      ConditionsList.Add(a);
    foreach(var a in sudoku.Select((v, i) => new { v, i }))
      foreach(var b in a.v.Select((v, i) => new { v, i }))
        if(!string.IsNullOrWhiteSpace(b.v))
          ConditionsList.Add(new List<string>() { a.i.ToString() + b.i.ToString() + int.Parse(b.v).ToString() });

    /**************************************************
     * 結果を返す
     **************************************************/

    result = new int[N, N];
    DefaultSolver solver = new DefaultSolver(ConditionsList);
    if(solver.Solve())
    {
      var d = solver.Model;
      foreach(var val in d.Where(_d => _d.Value == true).Select((v, i) => new { v, i }))
        result[int.Parse(val.v.Key[0].ToString()), int.Parse(val.v.Key[1].ToString())] = int.Parse(val.v.Key[2].ToString());
      return true;
    }
    else
      Console.WriteLine("問題が解けませんでした。。。");

    return false;
  }

  /// <summary>
  /// ソルバに渡した条件を列挙する
  /// </summary>
  public void ShowConditions()
  {
    foreach(var a in ConditionsList.Select((v, i) => new { v, i }))
    {
      Console.Write(a.i + 1 + " :{ ");
      foreach(var b in a.v)
        Console.Write(b + " ,");
      Console.WriteLine(" }");
    }
  }

  private static List<List<T>> Combinations<T>(IList<T> elements, int choose)
  {
    var ret = new List<List<T>>();

    // 再帰呼び出しの終端
    if(elements.Count < choose)
      return new List<List<T>>();
    else if(choose <= 0)
      return new List<List<T>>() { new List<T>() };

    // 本体
    for(var n = 1; n <= elements.Count; n++)
    {
      var subRet = Combinations(elements.Skip(n).ToList(), choose - 1);
      subRet.ForEach(e => e.Add(elements[n - 1]));
      ret.AddRange(subRet);
    }

    return ret;
  }
}
